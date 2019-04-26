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
    [ReplicateType]
    [ReplicateRoute(Route = "cards")]
    public class CardsInterface
    {
        IServiceProvider Services;
        public CardsInterface(IServiceProvider services)
        {
            Services = services;
        }

        [AuthRequired]
        public Task Update(CardData update)
        {
            return Services.DoWithDB(async db =>
            {
                var card = await db.Cards.Include(c => c.Tags).BoardFilter(update.Id).FirstOrDefaultAsync();
                if (card == null) throw new HTTPError("Card not found", 404);
                if (card.AuthorId != Auth.CurrentUser.Id)
                    Auth.VerifyPrivilege(Privilege.Developer);
                TypeUtil.UpdateMembersFrom(card, update,
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

        [AuthRequired(RequirePrivilege = Privilege.Contribute)]
        public Task<CardData> Add(CardData card)
        {
            return Services.DoWithDB(async db =>
            {
                card.Id = 0;
                card.BoardId = BoardExtensions.Board.Id;
                card.AuthorId = Auth.CurrentUser.Id;
                var column = db.Columns.BoardFilter().FirstOrDefault(col => col.Id == card.ColumnId);
                if (column == null)
                    throw new HTTPError("Invalid column id", 400);
                if (!column.AllowContribution)
                    Auth.VerifyPrivilege(Privilege.Developer);
                var result = await db.Cards.AddAsync(card);
                await db.Insert(card, card.Index, db.Cards.Where(c => c.BoardId == card.BoardId));
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        [AuthRequired(RequirePrivilege = Privilege.Admin)]
        public Task<bool> Delete(DeleteData card)
        {
            return Services.DoWithDB(
                db => db.DeleteOrderAsync(db.Cards, card.Id),
                false);
        }

        [AuthRequired(RequirePrivilege = Privilege.Vote)]
        public Task Vote(CardVoteRequest request)
        {
            return Services.DoWithDB(async db =>
            {
                var userId = Auth.CurrentUser.Id;
                var vote = db.Votes.FirstOrDefault(v => v.UserId == userId && v.CardId == request.CardId && v.BoardId == BoardExtensions.Board.Id)
                ?? (await db.Votes.AddAsync(new VoteData() { CardId = request.CardId, UserId = userId, Count = 0, BoardId = BoardExtensions.Board.Id })).Entity;
                vote.Count += request.Count;
                if (vote.Count <= 0)
                    db.Votes.Remove(vote);
                await db.SaveChangesAsync();
                return true;
            });
        }
    }
}
