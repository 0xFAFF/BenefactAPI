using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using Replicate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BenefactAPI.RPCInterfaces.Board
{
    [ReplicateType]
    public class CardQuery
    {
        public Dictionary<string, List<CardQueryTerm>> Groups;
        public int? Limit;
    }
    [ReplicateType]
    public class CardQueryTerm
    {
        public List<int> Tags;
        public CardState? State;
        public int? ColumnId;
        public bool ShowArchived;
        public string Title;
    }
    [ReplicateType]
    public class BoardResponse
    {
        public Dictionary<string, List<CardData>> Cards;
        public string Description;
        public Privilege? UserPrivilege;
        public List<ColumnData> Columns;
        public List<TagData> Tags;
        public List<UserData> Users;
        public Privilege? DefaultPrivilege;
        public string Title;
        public string UrlName;
    }
    [ReplicateType]
    public class SetPrivilegeRequest
    {
        public int UserId;
        public Privilege Privilege;
    }
    [ReplicateType]
    public class CreateInviteRequest
    {
        public Privilege Privilege;
    }
    [ReplicateType]
    public class JoinRequest
    {
        public string Key;
    }

    [ReplicateType]
    public class BoardsInterface
    {
        IServiceProvider Services;
        public BoardsInterface(IServiceProvider services)
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
            if (term.State.HasValue)
                andTerms.Add(card => card.Column.State == term.State);
            if (!term.ShowArchived)
                andTerms.Add(card => card.Archived == false);
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
            else
            {
                query = query.Where(c => c.Archived == false);
            }
            return query;
        }

        [AuthRequired]
        [ReplicateRoute(Route = "/")]
        public Task<BoardResponse> Get(CardQuery query)
        {
            var response = new BoardResponse();
            TypeUtil.CopyFrom(response, BoardExtensions.Board, blackList:
                new string[] { nameof(BoardResponse.Tags), nameof(BoardResponse.Columns), nameof(BoardResponse.Users), nameof(BoardResponse.Cards) });
            if (Auth.CurrentRole == null)
                return Task.FromResult(response);
            query = query ?? new CardQuery();
            query.Groups = query.Groups ?? new Dictionary<string, List<CardQueryTerm>>() { { "All", null } };
            var boardId = BoardExtensions.Board.Id;
            return Services.DoWithDB(async db =>
            {
                IQueryable<CardData> baseQuery = db.Cards
                    .BoardFilter()
                    .Include(card => card.Tags)
                    .Include(card => card.Comments)
                    .Include(card => card.Votes)
                    .Include(card => card.Attachments)
                    .OrderBy(card => card.Index);
                var cardGroups = new Dictionary<string, List<CardData>>();
                // TODO: This is derpy and serial, but the EF Core Include seems to have a bug in it when the queries run simultanesouly
                // which duplicates Tags in CardData
                foreach (var group in query.Groups)
                {
                    cardGroups[group.Key] = await FilterCards(baseQuery, group.Value).ToListAsync();
                }
                response.Cards = cardGroups;
                response.Columns = await db.Columns.BoardFilter().OrderBy(col => col.Index).ToListAsync();
                response.Tags = await db.Tags.BoardFilter().OrderBy(tag => tag.Id).ToListAsync();
                response.UserPrivilege = Auth.CurrentRole?.Privilege;
                response.Users = await db.Users
                .Where(u => u.Roles.Any(p => p.BoardId == boardId) || u.Votes.Any(v => v.BoardId == boardId) || u.Comments.Any(c => c.BoardId == boardId))
                .ToListAsync();
                return response;
            });
        }
        public static async Task<UserRole> GetOrCreateRole(BenefactDbContext db, int userId)
        {
            if (userId == BoardExtensions.Board.CreatorId)
                throw new HTTPError("Cannot set the privilege of the board's creator", 400);
            var existingRole = await db.Roles.Where(r => r.BoardId == BoardExtensions.Board.Id && r.UserId == userId).FirstOrDefaultAsync();
            if (existingRole == null)
                existingRole = (await db.Roles.AddAsync(new UserRole()
                {
                    BoardId = BoardExtensions.Board.Id,
                    Privilege = Privilege.None,
                    UserId = userId
                })).Entity;
            return existingRole;
        }
        [AuthRequired(RequirePrivilege = Privilege.Admin)]
        public async Task<bool> SetPrivilege(SetPrivilegeRequest request)
        {
            return await Services.DoWithDB(async db =>
            {
                var role = await GetOrCreateRole(db, request.UserId);
                if (role.Privilege == request.Privilege)
                    return false;
                role.Privilege = request.Privilege;
                return true;
            });
        }
        const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        [AuthRequired(RequirePrivilege = Privilege.Admin)]
        public async Task<string> Invite(CreateInviteRequest request)
        {
            return await Services.DoWithDB(async db =>
            {
                var existingInvite = await db.Invites.Where(i => i.BoardId == BoardExtensions.Board.Id && i.Privilege == request.Privilege).FirstOrDefaultAsync();
                if (existingInvite == null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        try
                        {
                            Random r = new Random();
                            var newInvite = (await db.Invites.AddAsync(new InviteData()
                            {
                                BoardId = BoardExtensions.Board.Id,
                                Privilege = request.Privilege,
                                Key = new string(Enumerable.Range(0, 10).Select(_ => alphabet[r.Next(62)]).ToArray())
                            })).Entity;
                            await db.SaveChangesAsync();
                            return newInvite.Key;
                        }
                        catch { }
                    }
                    throw new HTTPError("Failed to create invite");
                }
                return existingInvite.Key;
            });
        }
        public async Task<UserRole> Join(JoinRequest request)
        {
            return await Services.DoWithDB(async db =>
            {
                var privilege = BoardExtensions.Board.DefaultPrivilege;
                if (request?.Key != null)
                {
                    var invite = await db.Invites.Include(i => i.Board).Where(i => i.Key == request.Key)
                        .FirstOrError("Invalid invite key", 400);
                    privilege = (privilege ?? Privilege.None) | invite.Privilege;
                }
                if (privilege == null)
                    throw new HTTPError("Cannot join private board");
                var role = await GetOrCreateRole(db, Auth.CurrentUser.Id);
                role.Privilege |= privilege.Value;
                return role;
            });
        }
    }
}
