using BenefactAPI.DataAccess;
using BenefactAPI.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Replicate.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
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

        public async Task<T> DoWithDB<T>(Func<BenefactDBContext, Task<T>> func)
        {
            using (var scope = Services.CreateScope())
            using (var db = scope.ServiceProvider.GetService<BenefactDBContext>())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                var result = await func(db);
                transaction.Commit();
                return result;
            }
        }

        public Task<CardsResponse> Cards()
        {
            return DoWithDB(async db =>
            {
                return new CardsResponse()
                {
                    Cards = await db.Cards.OrderBy(card => card.Index).Include(c => c.Tags).ToListAsync(),
                    Columns = await db.Columns.OrderBy(col => col.Index).ToListAsync(),
                    Tags = await db.Tags.OrderBy(tag => tag.Id).ToListAsync(),
                };
            });
        }

        async Task Insert<T>(T value, int? newIndex, IQueryable<T> existingSet, BenefactDBContext db) where T : IOrdered
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

        public Task UpdateCard(CardData update)
        {
            return DoWithDB(async db =>
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
            return DoWithDB(async db =>
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
            return DoWithDB(async db =>
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
            return DoWithDB(async db =>
            {
                tag.Id = 0;
                var result = await db.Tags.AddAsync(tag);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        public Task<bool> DeleteTag(DeleteData tag)
        {
            return DoWithDB(async db =>
            {
                return await Delete(db, db.Tags, new TagData() { Id = tag.Id });
            });
        }

        public Task UpdateTag(TagData tag)
        {
            return DoWithDB(async db =>
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
            return DoWithDB(async db =>
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
            return DoWithDB(async db =>
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
            return DoWithDB(async db =>
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

        async Task<bool> Delete<T>(BenefactDBContext db, DbSet<T> set, T delete) where T : class
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
