using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.RPCInterfaces
{
    [ReplicateType]
    public class CreateBoardRequest
    {
        public string Title;
        public string UrlName;
    }
    [ReplicateType]
    public class TrelloImportRequest
    {
        public string UrlName;
        public TrelloBoard Board;
    }
    [ReplicateType]
    public class MetaInterface
    {
        IServiceProvider services;
        public MetaInterface(IServiceProvider services)
        {
            this.services = services;
        }
        static async Task addAdminRole(BenefactDbContext db, BoardData board)
        {
            var userRole = new UserRole() { BoardId = board.Id, UserId = Auth.CurrentUser.Id, Privilege = (Privilege)255 };
            await db.AddAsync(userRole);
        }
        [AuthRequired]
        [ReplicateRoute(Route = "create_board")]
        public Task<string> Create(CreateBoardRequest request)
        {
            return services.DoWithDB(async db =>
            {
                var board = new BoardData();
                TypeUtil.UpdateMembersFrom(board, request);
                board.CreatorId = Auth.CurrentUser.Id;
                await db.AddAsync(board);
                await addAdminRole(db, board);
                return board.UrlName;
            }).HandleDuplicate("ix_boards_url_name", "A board with that URL already exists");
        }
        [AuthRequired]
        [ReplicateRoute(Route = "trello_import")]
        public Task<string> Import(TrelloImportRequest request)
        {
            var board = request.Board;
            return services.DoWithDB(async db =>
            {
                board.Board = new BoardData()
                {
                    Title = board.name,
                    UrlName = request.UrlName,
                    CreatorId = Auth.CurrentUser.Id,
                };
                await db.AddAsync(board.Board);
                foreach (var label in board.labels)
                {
                    label.Tag = new TagData()
                    {
                        BoardId = board.Board.Id,
                        Color = label.color,
                        Name = label.name
                    };
                    await db.AddAsync(label.Tag);
                }
                for (int i = 0; i < board.lists.Count; i++)
                {
                    var list = board.lists[i];
                    list.Column = new ColumnData()
                    {
                        BoardId = board.Board.Id,
                        Title = list.name,
                        Index = i,
                    };
                    await db.AddAsync(list.Column);
                }
                var cards = board.cards.Where(c => !c.closed).ToList();
                for (int i = 0; i < cards.Count; i++)
                {
                    var card = cards[i];
                    card.Card = new CardData()
                    {
                        BoardId = board.Board.Id,
                        ColumnId = board.lists.First(l => l.id == card.idList).Column.Id,
                        Description = card.desc,
                        TagIds = card.idLabels.Select(lid => board.labels.First(l => l.id == lid).Tag.Id).ToList(),
                        Title = card.name,
                        Index = i,
                        AuthorId = Auth.CurrentUser.Id,
                    };
                    await db.AddAsync(card.Card);
                    foreach (var attachment in card.attachments)
                    {
                        await db.AddAsync(new AttachmentData()
                        {
                            BoardId = board.Board.Id,
                            CardId = card.Card.Id,
                            Name = attachment.name,
                            Url = attachment.url,
                            Preview = attachment.previews.FirstOrDefault(p => p.width == 150)?.url,
                            UserId = Auth.CurrentUser.Id,
                        });
                    }
                }
                await addAdminRole(db, board.Board);
                return board.Board.UrlName;
            }).HandleDuplicate("ix_boards_url_name", "A board with that URL already exists");
        }
    }
}
