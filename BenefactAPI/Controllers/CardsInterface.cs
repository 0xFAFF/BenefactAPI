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
    public class CardsInterface
    {
        IServiceProvider Services;
        public CardsInterface(IServiceProvider services)
        {
            Services = services;
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
            IQueryable<CardData> baseQuery = db.Cards
                .Include(card => card.Tags)
                .Include(card => card.Comments)
                .Include(card => card.Votes)
                .OrderBy(card => card.Index);
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
                {
                    cardGroups[group.Key] = await QueryCards(db, group.Value).ToListAsync();
                }
                return new CardsResponse()
                {
                    Cards = cardGroups,
                    Columns = await db.Columns.OrderBy(col => col.Index).ToListAsync(),
                    Tags = await db.Tags.OrderBy(tag => tag.Id).ToListAsync(),
                    Users = await db.Users.ToListAsync(),
                };
            });
        }

        [AuthRequired]
        public Task UpdateCard(CardData update)
        {
            return Services.DoWithDB(async db =>
            {
                var existingCard = await db.Cards.Include(c => c.Tags).FirstOrDefaultAsync(c => c.Id == update.Id);
                if (existingCard == null) throw new HTTPError("Card not found");
                Util.UpdateMembersFrom(existingCard, update,
                    whiteList: new[] { nameof(CardData.Title), nameof(CardData.Description), nameof(CardData.ColumnId) });
                if (update.TagIds != null)
                {
                    existingCard.Tags.Clear();
                    existingCard.TagIds = update.TagIds;
                }
                if (update.Index.HasValue)
                    await db.Insert(existingCard, update.Index.Value, db.Cards);
                await db.SaveChangesAsync();
                return true;
            });
        }

        [AuthRequired]
        public Task<CardData> AddCard(CardData card)
        {
            return Services.DoWithDB(async db =>
            {
                card.Id = 0;
                var result = await db.Cards.AddAsync(card);
                // TODO: Filter this db.Cards when there are boards
                await db.Insert(card, card.Index, db.Cards);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        [AuthRequired]
        public Task<bool> DeleteCard(DeleteData card)
        {
            return Services.DoWithDB(async db =>
            {
                if (await db.Delete(db.Cards, new CardData() { Id = card.Id }))
                {
                    await db.Order(db.Cards);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            });
        }

        [AuthRequired]
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

        [AuthRequired]
        public Task<bool> DeleteTag(DeleteData tag)
        {
            return Services.DoWithDB(async db =>
            {
                return await db.Delete(db.Tags, new TagData() { Id = tag.Id });
            });
        }

        [AuthRequired]
        public Task UpdateTag(TagData tag)
        {
            return Services.DoWithDB(async db =>
            {
                var existingCard = await db.Tags.FindAsync(tag.Id);
                if (existingCard == null) throw new HTTPError("Tag not found");
                Util.UpdateMembersFrom(existingCard, tag, blackList: new[] { nameof(TagData.Id) });
                await db.SaveChangesAsync();
                return true;
            });
        }

        [AuthRequired]
        public Task<ColumnData> AddColumn(ColumnData column)
        {
            return Services.DoWithDB(async db =>
            {
                column.Id = 0;
                var result = await db.Columns.AddAsync(column);
                await db.Insert(column, column.Index, db.Columns);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        [AuthRequired]
        public Task<bool> DeleteColumn(DeleteData column)
        {
            return Services.DoWithDB(async db =>
            {
                if (await db.Delete(db.Columns, new ColumnData() { Id = column.Id }))
                {
                    await db.Order(db.Columns);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            });
        }

        [AuthRequired]
        public Task UpdateColumn(ColumnData update)
        {
            return Services.DoWithDB(async db =>
            {
                var column = await db.Columns.FindAsync(update.Id);
                if (column == null) throw new HTTPError("Column not found");
                Util.UpdateMembersFrom(column, update, whiteList: new[] { nameof(ColumnData.Title) });
                if (update.Index.HasValue)
                    await db.Insert(column, update.Index.Value, db.Columns);
                await db.SaveChangesAsync();
                return true;
            });
        }

        [AuthRequired]
        public Task CardVote(CardVoteRequest request)
        {
            return Services.DoWithDB(async db =>
            {
                var userId = Auth.CurrentUser.Value.Id;
                var vote = db.Votes.FirstOrDefault(v => v.UserId == userId && v.CardId == request.CardId)
                ?? (await db.Votes.AddAsync(new VoteData() { CardId = request.CardId, UserId = userId, Count = 0 })).Entity;
                vote.Count += request.Count;
                if (vote.Count <= 0)
                    db.Votes.Remove(vote);
                await db.SaveChangesAsync();
                return true;
            });
        }
    }
}
