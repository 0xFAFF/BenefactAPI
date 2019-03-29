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
    [ReplicateRoute(Route = "cards")]
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

        IQueryable<CardData> FilterCards(IQueryable<CardData> baseQuery, List<CardQueryTerm> terms)
        {
            var query = baseQuery;
            if (terms != null)
            {
                var exp = terms.SelectExp(termCardExpression).BinaryCombinator(Expression.Or);
                query = query.Where(exp);
            }
            return query;
        }

        [ReplicateRoute(Route = "/")]
        public Task<CardsResponse> Get(CardQuery query)
        {
            query = query ?? new CardQuery();
            query.Groups = query.Groups ?? new Dictionary<string, List<CardQueryTerm>>() { { "All", null } };
            return Services.DoWithDB(async db =>
            {
                IQueryable<CardData> baseQuery = db.Cards
                    .Include(card => card.Tags)
                    .Include(card => card.Comments)
                    .Include(card => card.Votes)
                    .Where(c => c.BoardId == query.BoardId)
                    .OrderBy(card => card.Index);
                var cardGroups = new Dictionary<string, List<CardData>>();
                // TODO: This is derpy and serial, but the EF Core Include seems to have a bug in it when the queries run simultanesouly
                // which duplicates Tags in CardData
                foreach (var group in query.Groups)
                {
                    cardGroups[group.Key] = await FilterCards(baseQuery, group.Value).ToListAsync();
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
        public Task Update(CardData update)
        {
            return Services.DoWithDB(async db =>
            {
                var card = await db.Cards.Include(c => c.Tags).FirstOrDefaultAsync(c => c.Id == update.Id);
                if (card == null) throw new HTTPError("Card not found");
                Util.UpdateMembersFrom(card, update,
                    whiteList: new[] { nameof(CardData.Title), nameof(CardData.Description), nameof(CardData.ColumnId) });
                if (update.TagIds != null)
                {
                    card.Tags.Clear();
                    card.TagIds = update.TagIds;
                }
                if (update.Index.HasValue)
                    await db.Insert(card, update.Index.Value, db.Cards.Where(c => c.BoardId == card.BoardId));
                await db.SaveChangesAsync();
                return true;
            });
        }

        [AuthRequired]
        public Task<CardData> Add(CardData card)
        {
            return Services.DoWithDB(async db =>
            {
                card.Id = 0;
                var result = await db.Cards.AddAsync(card);
                await db.Insert(card, card.Index, db.Cards.Where(c => c.BoardId == card.BoardId));
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        [AuthRequired]
        public Task<bool> Delete(DeleteData card)
        {
            return Services.DoWithDB(
                db => db.DeleteAndOrder(db.Cards, card.Id, deleted => c => c.BoardId == deleted.BoardId),
                false);
        }

        [AuthRequired]
        public Task Vote(CardVoteRequest request)
        {
            return Services.DoWithDB(async db =>
            {
                var userId = Auth.CurrentUser.Id;
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
