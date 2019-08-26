using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using BenefactAPI.RPCInterfaces.Board;
using Microsoft.EntityFrameworkCore;
using Replicate;
using Replicate.Web;
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
        public bool CreateTemplate;
    }
    [ReplicateType]
    public class TrelloImportRequest
    {
        public string UrlName;
        public TrelloBoard Board;
    }
    [ReplicateType]
    [ReplicateRoute(Route = "api")]
    public class MetaInterface
    {
        IServiceProvider services;

        public Task<string> Version()
        {
            return Task.FromResult(Environment.GetEnvironmentVariable("GIT_COMMIT"));
        }
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
        [ReplicateRoute(Route = "board/create")]
        public Task<string> Create(CreateBoardRequest request)
        {
            return services.DoWithDB(async db =>
            {
                var board = new BoardData();
                TypeUtil.CopyFrom(board, request);
                board.CreatorId = Auth.CurrentUser.Id;
                if (request.CreateTemplate)
                {
                    List<ColumnData> columns = new List<ColumnData>
                    {
                        new ColumnData
                        {
                            BoardId = board.Id,
                            Title = "To Do",
                            Index = 0,
                            AllowContribution = false,
                            State = CardState.Proposed
                        },
                        new ColumnData
                        {
                            BoardId = board.Id,
                            Title = "In Progress",
                            Index = 1,
                            AllowContribution = false,
                            State = CardState.InProgress
                        },
                        new ColumnData
                        {
                            BoardId = board.Id,
                            Title = "Complete",
                            Index = 2,
                            AllowContribution = false,
                            State = CardState.Complete
                        }
                    };
                    List<TagData> tags = new List<TagData>
                    {
                        new TagData
                        {
                            BoardId = board.Id,
                            Name = "Bug",
                            Color = "#FF4136",
                        },
                        new TagData
                        {
                            BoardId = board.Id,
                            Name = "Duplicate",
                            Color = "#001f3f",
                        },
                        new TagData
                        {
                            BoardId = board.Id,
                            Name = "Enhancement",
                            Color = "#7FDBFF",
                        },
                        new TagData
                        {
                            BoardId = board.Id,
                            Name = "Help Wanted",
                            Color = "#3D9970",
                        },
                        new TagData
                        {
                            BoardId = board.Id,
                            Name = "Invalid",
                            Color = "#85144B",
                        },
                        new TagData
                        {
                            BoardId = board.Id,
                            Name = "Question",
                            Color = "#FF851B",
                        },
                        new TagData
                        {
                            BoardId = board.Id,
                            Name = "Wont Fix",
                            Color = "#F012BE",
                        }
                    };

                    List<CardData> cards = new List<CardData>
                    {
                        new CardData
                        {
                            BoardId = board.Id,
                            Column = columns[0],
                            Description = "This is a description.",
                            Title = "Example Card",
                            Index = 0,
                            AuthorId = Auth.CurrentUser.Id
                        }
                    };

                    foreach (var column in columns)
                    {
                        board.Columns.Add(column);
                    }

                    foreach (var tag in tags)
                    {
                        board.Tags.Add(tag);
                    }

                    foreach (var card in cards)
                    {
                        board.Cards.Add(card);
                    }
                }

                await db.AddAsync(board);
                await addAdminRole(db, board);
                return board.UrlName;
            }).HandleDuplicate("ix_boards_url_name", "A board with that URL already exists");
        }
        [AuthRequired]
        [ReplicateRoute(Route = "board/trello_import")]
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
                        AllowContribution = false,
                        State = CardState.Proposed,
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
