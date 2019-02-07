using BenefactAPI.DataAccess;
using BenefactAPI.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Replicate.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Replicate;

namespace BenefactAPI.Controllers
{
    [ReplicateType(AutoMethods = AutoAdd.AllPublic)]
    public interface ICardsInterface
    {
        Task<CardsResponse> Cards(CardQuery query);
        /// <summary>
        /// Update a card with the non-null fields provided in CardFields
        /// </summary>
        /// <param name="update"></param>
        Task UpdateCard(CardData update);
        Task<CardData> AddCard(CardData card);
        Task<bool> DeleteCard(DeleteData card);

        Task<TagData> AddTag(TagData tag);
        Task UpdateTag(TagData tag);
        Task<bool> DeleteTag(DeleteData tag);

        Task<ColumnData> AddColumn(ColumnData column);
        Task UpdateColumn(ColumnData column);
        Task<bool> DeleteColumn(DeleteData column);
    }
    public class CardsInterface : ICardsInterface
    {
        IServiceProvider Services;
        public CardsInterface(IServiceProvider services)
        {
            Services = services;
        }

        void UpdateMembersFrom<T>(T target, T newFields, params string[] ignoredFields)
        {
            var td = ReplicationModel.Default.GetTypeAccessor(typeof(T));
            IEnumerable<MemberAccessor> members = td.MemberAccessors;
            if (ignoredFields != null)
                members = members.Where(mem => !ignoredFields.Contains(mem.Info.Name));
            foreach (var member in members)
            {
                var newValue = member.GetValue(newFields);
                if (newValue == null) continue;
                member.SetValue(target, newValue);
            }
        }

        Expression<Func<CardData, bool>> termCardExpression(CardQueryTerm term)
        {
            var andTerms = new List<Expression<Func<CardData, bool>>>();
            if (term.Tags != null)
            {
                andTerms.AddRange(term.Tags.SelectExp<int, CardData, bool>(
                    tagId => card => card.Tags.Any(cardTag => cardTag.TagId == tagId)));
            }
            if (term.Title != null)
                andTerms.Add(card => card.Title.ToLower().Contains(term.Title.ToLower()));
            if (term.ColumnId.HasValue)
                andTerms.Add(card => card.Column.Id == term.ColumnId);
            if (!andTerms.Any())
                andTerms.Add(c => true);
            return andTerms.BinaryCombinator(Expression.And);
        }

        IQueryable<CardData> QueryCards(BenefactDbContext db, List<CardQueryTerm> terms)
        {
            IQueryable<CardData> baseQuery = db.Cards.Include(card => card.Tags).OrderBy(card => card.Index);
            var query = baseQuery;
            if (terms != null)
            {
                var exp = terms.SelectExp(termCardExpression).BinaryCombinator(Expression.Or);
                query = query.Where(exp);
            }
            return query;
        }

        public Task<CardsResponse> Cards(CardQuery query)
        {
            query = query ?? new CardQuery() { Groups = new Dictionary<string, List<CardQueryTerm>>() { { "All", null } } };
            return Services.DoWithDB(async db =>
            {
                var cardGroups = new Dictionary<string, List<CardData>>();
                // TODO: This is derpy and serial, but the EF Core Include seems to have a bug in it when the queries run simultanesouly
                // which duplicates Tags in CardData
                foreach (var group in query.Groups)
                    cardGroups[group.Key] = await QueryCards(db, group.Value).ToListAsync();
                return new CardsResponse()
                {
                    Cards = cardGroups,
                    Columns = await db.Columns.OrderBy(col => col.Index).ToListAsync(),
                    Tags = await db.Tags.OrderBy(tag => tag.Id).ToListAsync(),
                };
            });
        }

        async Task Insert<T>(T value, int? newIndex, IQueryable<T> existingSet, BenefactDbContext db) where T : IOrdered
        {
            var max = await existingSet.CountAsync();

            if (value.Index == null)
                value.Index = max;
            if (newIndex == null)
                newIndex = max;
            if (newIndex == value.Index)
                return;
            newIndex = Math.Min(Math.Max(0, newIndex.Value), max);
            var movingEarlier = newIndex < value.Index;
            var startIndex = (movingEarlier ? newIndex : value.Index).Value;
            var endIndex = (movingEarlier ? value.Index : newIndex).Value;
            var greaterList = await existingSet.Where(v => v.Index.Value >= startIndex && v.Index.Value <= endIndex)
                .ToListAsync();
            foreach (var greaterItem in greaterList)
            {
                greaterItem.Index += movingEarlier ? 1 : -1;
            }
            value.Index = newIndex;
            await db.SaveChangesAsync();
            await Order(existingSet);
        }
        async Task Order<T>(IQueryable<T> existingSet) where T : IOrdered
        {
            var allItems = await existingSet.OrderBy(v => v.Index).ToListAsync();
            foreach (var tuple in allItems.Select((item, index) => new { item, index }))
                tuple.item.Index = tuple.index;
        }

        [AuthRequired]
        public Task UpdateCard(CardData update)
        {
            return Services.DoWithDB(async db =>
            {
                var existingCard = await db.Cards.Include(c => c.Tags).FirstOrDefaultAsync(c => c.Id == update.Id);
                if (existingCard == null) throw new HTTPError("Card not found");
                UpdateMembersFrom(existingCard, update, nameof(CardData.Id), nameof(CardData.TagIds), nameof(CardData.Index));
                if (update.TagIds != null)
                {
                    existingCard.Tags.Clear();
                    existingCard.TagIds = update.TagIds;
                }
                if (update.Index.HasValue)
                    await Insert(existingCard, update.Index.Value, db.Cards, db);
                await db.SaveChangesAsync();
                return true;
            });
        }

        public Task<CardData> AddCard(CardData card)
        {
            return Services.DoWithDB(async db =>
            {
                card.Id = 0;
                var result = await db.Cards.AddAsync(card);
                // TODO: Filter this db.Cards when there are boards
                await Insert(card, card.Index, db.Cards, db);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        public Task<bool> DeleteCard(DeleteData card)
        {
            return Services.DoWithDB(async db =>
            {
                if (await Delete(db, db.Cards, new CardData() { Id = card.Id }))
                {
                    await Order(db.Cards);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            });
        }

        public Task<TagData> AddTag(TagData tag)
        {
            return Services.DoWithDB(async db =>
            {
                tag.Id = 0;
                var result = await db.Tags.AddAsync(tag);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        public Task<bool> DeleteTag(DeleteData tag)
        {
            return Services.DoWithDB(async db =>
            {
                return await Delete(db, db.Tags, new TagData() { Id = tag.Id });
            });
        }

        public Task UpdateTag(TagData tag)
        {
            return Services.DoWithDB(async db =>
            {
                var existingCard = await db.Tags.FindAsync(tag.Id);
                if (existingCard == null) throw new HTTPError("Tag not found");
                UpdateMembersFrom(existingCard, tag, nameof(TagData.Id));
                await db.SaveChangesAsync();
                return true;
            });
        }

        public Task<ColumnData> AddColumn(ColumnData column)
        {
            return Services.DoWithDB(async db =>
            {
                column.Id = 0;
                var result = await db.Columns.AddAsync(column);
                await Insert(column, column.Index, db.Columns, db);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        public Task<bool> DeleteColumn(DeleteData column)
        {
            return Services.DoWithDB(async db =>
            {
                if (await Delete(db, db.Columns, new ColumnData() { Id = column.Id }))
                {
                    await Order(db.Columns);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            });
        }

        public Task UpdateColumn(ColumnData update)
        {
            return Services.DoWithDB(async db =>
            {
                var column = await db.Columns.FindAsync(update.Id);
                if (column == null) throw new HTTPError("Column not found");
                UpdateMembersFrom(column, update, nameof(ColumnData.Id), nameof(ColumnData.Index));
                if (update.Index.HasValue)
                    await Insert(column, update.Index.Value, db.Columns, db);
                await db.SaveChangesAsync();
                return true;
            });
        }

        async Task<bool> Delete<T>(BenefactDbContext db, DbSet<T> set, T delete) where T : class
        {
            set.Remove(delete);
            try
            {
                await db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException e)
            {
                return false;
            }
        }
    }
}
