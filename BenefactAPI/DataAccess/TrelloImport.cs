using BenefactAPI.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Replicate;
using Replicate.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.DataAccess
{
    [ReplicateType]
    public class TrelloCard
    {
        public string id;
        public bool closed;
        public List<string> idLabels;
        public string idList;
        public string desc;
        public string name;

        [ReplicateIgnore]
        public CardData Card;
    }
    [ReplicateType]
    public class TrelloList
    {
        public string id;
        public string name;

        [ReplicateIgnore]
        public ColumnData Column;
    }
    [ReplicateType]
    public class TrelloLabel
    {
        public string id;
        public string name;
        public string color;

        [ReplicateIgnore]
        public TagData Tag;
    }
    [ReplicateType]
    public class TrelloBoard
    {
        public string id;
        public string name;
        public List<TrelloLabel> labels;
        public List<TrelloCard> cards;
        public List<TrelloList> lists;

        [ReplicateIgnore]
        public BoardData Board;
    }
    public class TrelloImport
    {
        public static Task<string> Import(TrelloBoard board, IServiceProvider services)
        {
            return services.DoWithDB(async db =>
            {
                board.Board = new BoardData()
                {
                    Title = board.name,
                    UrlName = board.name.ToLower(),
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
                }
                var role = new BoardRole() { Name = "Admin", Privilege = (Privilege)255, BoardId = board.Board.Id };
                await db.AddAsync(role);
                var userRole = new UserBoardRole() { BoardId = board.Board.Id, UserId = Auth.CurrentUser.Id, BoardRole = role };
                await db.AddAsync(userRole);
                return board.Board.UrlName;
            });
        }
    }
}
