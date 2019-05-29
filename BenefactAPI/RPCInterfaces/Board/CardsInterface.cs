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

namespace BenefactAPI.RPCInterfaces.Board
{
    [ReplicateType]
    public class CardVoteRequest
    {
        public int Count;
        public int CardId;
    }
    [ReplicateType]
    public class CardArchiveRequest
    {
        public int CardId;
        public bool Archive = true;
    }
    [ReplicateType]
    public class CardAssignRequest
    {
        public int CardId;
        public int? AssigneeId;
    }
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
                // Only developers can edit cards they don't own, or move cards
                if (card.AuthorId != Auth.CurrentUser.Id || update.Index.HasValue || update.ColumnId.HasValue)
                    Auth.VerifyPrivilege(Privilege.Developer);
                // Verify the assignee is valid and the editor is a developer
                if (update.AssigneeId.HasValue)
                {
                    Auth.VerifyPrivilege(Privilege.Developer);
                    if (update.AssigneeId != 0)
                        await db.Roles.Where(
                            r => r.BoardId == BoardExtensions.Board.Id
                                && r.UserId == update.AssigneeId
                                && (r.Privilege & Privilege.Developer) != 0)
                            .FirstOrError("Invalid assignee", 400);
                    card.AssigneeId = update.AssigneeId == 0 ? null : update.AssigneeId;
                }
                TypeUtil.CopyFrom(card, update,
                    whiteList: new[] { nameof(CardData.Title), nameof(CardData.Description),
                        nameof(CardData.ColumnId) });
                if (update.Tags != null)
                {
                    card.Tags.Clear();
                    card.TagIds = update.TagIds;
                }
                if (update.Index.HasValue)
                    await db.Insert(card, update.Index.Value, db.Cards.Where(c => c.BoardId == card.BoardId));
                await Activity.LogActivity(db, card, ActivityType.Update);
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
                card.Archived = false;
                var column = db.Columns.BoardFilter().FirstOrDefault(col => col.Id == card.ColumnId);
                if (column == null)
                    throw new HTTPError("Invalid column id", 400);
                if (!column.AllowContribution)
                    Auth.VerifyPrivilege(Privilege.Developer);
                var result = await db.Cards.AddAsync(card);
                await db.Insert(card, card.Index, db.Cards.Where(c => c.BoardId == card.BoardId));
                await Activity.LogActivity(db, card, ActivityType.Create);
                return result.Entity;
            });
        }

        [AuthRequired(RequirePrivilege = Privilege.Admin)]
        public Task<bool> Delete(IDRequest card)
        {
            return Services.DoWithDB(
                async db =>
                {
                    var existing = await db.Cards.FirstOrDefaultAsync(c => c.Id == card.Id);
                    return await db.DeleteOrderAsync(db.Cards, card.Id);
                },
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
        [AuthRequired]
        public Task Archive(CardArchiveRequest request)
        {
            return Services.DoWithDB(async db =>
            {
                var card = await db.Cards.BoardFilter(request.CardId).FirstOr404();
                if (card.AuthorId != Auth.CurrentUser.Id)
                    Auth.VerifyPrivilege(Privilege.Developer);
                card.Archived = request.Archive;
                await Activity.LogActivity(db, card, ActivityType.Archive);
            });
        }
    }
}
