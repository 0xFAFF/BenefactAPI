using Replicate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.DataAccess
{
    public interface IOrdered
    {
        int? Index { get; set; }
    }

    public interface IBoardId
    {
        int Id { get; }
        int BoardId { get; }
        BoardData Board { get; }
    }

    public interface ICardReference
    {
        int BoardId { get; }
        int CardId { get; }
        CardData Card { get; }
    }

    [ReplicateType(AutoMembers = AutoAdd.None)]
    public class BoardData
    {
        [Replicate]
        public int Id { get; set; }
        [Required]
        [Replicate]
        public string Title { get; set; }
        [Required]
        [Replicate]
        public string UrlName { get; set; }
        public UserData Creator { get; set; }
        public int? CreatorId { get; set; }
        public List<CardData> Cards { get; set; } = new List<CardData>();
        public List<CommentData> Comments { get; set; } = new List<CommentData>();
        public List<VoteData> Votes { get; set; } = new List<VoteData>();
        public List<ColumnData> Columns { get; set; } = new List<ColumnData>();
        public List<TagData> Tags { get; set; } = new List<TagData>();
        public List<UserBoardRole> Users { get; set; } = new List<UserBoardRole>();
        public List<BoardRole> Roles { get; set; } = new List<BoardRole>();
        public List<AttachmentData> Attachments { get; set; } = new List<AttachmentData>();
    }

    /// <summary>
    /// All fields on this must have null defaults since it's used in CardUpdate
    /// and specifying a non-null default will make it clear fields!
    /// </summary>
    [ReplicateType]
    public class CardData : IOrdered, IBoardId
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        [ReplicateIgnore]
        [Required]
        public UserData Author { get; set; }
        [Required]
        public int? Index { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [ReplicateIgnore]
        public BoardData Board { get; set; }
        [ReplicateIgnore]
        public int BoardId { get; set; }
        [ReplicateIgnore]
        public ColumnData Column { get; set; }
        [Required]
        public int? ColumnId { get; set; }
        [ReplicateIgnore]
        public List<CardTag> Tags { get; set; } = new List<CardTag>();
        [NotMapped]
        public List<int> TagIds
        {
            // TODO: Order by tag order?
            get => Tags.OrderBy(tag => tag.TagId).Select(ccd => ccd.TagId).ToList();
            set => Tags = value.Select(v => new CardTag() { Card = this, TagId = v }).ToList();
        }
        public ICollection<CommentData> Comments { get; set; } = new AutoSortList<double, CommentData>(c => -c.CreatedTime);
        public List<VoteData> Votes { get; set; } = new List<VoteData>();
        public List<AttachmentData> Attachments { get; set; } = new List<AttachmentData>();
    }
    [ReplicateType]
    public class CommentData : IBoardId, ICardReference
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int UserId { get; set; }
        [Required]
        [ReplicateIgnore]
        public UserData User { get; set; }
        public int CardId { get; set; }
        [ReplicateIgnore]
        public BoardData Board { get; set; }
        [ReplicateIgnore]
        public int BoardId { get; set; }
        [Required]
        [ReplicateIgnore]
        public CardData Card { get; set; }
        public double CreatedTime { get; set; } = Util.Now();
        public double? EditedTime { get; set; }
    }
    [ReplicateType]
    public class VoteData : ICardReference
    {
        public int Count { get; set; }
        public int UserId { get; set; }
        [Required]
        [ReplicateIgnore]
        public UserData User { get; set; }
        public int CardId { get; set; }
        [Required]
        [ReplicateIgnore]
        public CardData Card { get; set; }
        [ReplicateIgnore]
        public int BoardId { get; set; }
        [ReplicateIgnore]
        public BoardData Board { get; set; }
    }
    public class CardTag : ICardReference
    {
        public int BoardId { get; set; }
        public BoardData Board { get; set; }
        public int CardId { get; set; }
        public CardData Card { get; set; }
        public int TagId { get; set; }
        public TagData Tag { get; set; }
    }
    [ReplicateType]
    public class ColumnData : IOrdered, IBoardId
    {
        public int Id { get; set; }
        public int? Index { get; set; }
        public string Title { get; set; }
        public bool AllowContribution { get; set; }
        [ReplicateIgnore]
        public BoardData Board { get; set; }
        [ReplicateIgnore]
        public int BoardId { get; set; }

        [ReplicateIgnore]
        public List<CardData> Cards { get; set; }
    }
    [ReplicateType]
    public class TagData : IBoardId
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Character { get; set; }

        [ReplicateIgnore]
        public BoardData Board { get; set; }
        [ReplicateIgnore]
        public int BoardId { get; set; }

        [ReplicateIgnore]
        public List<CardTag> CardTags { get; set; }
    }
    [ReplicateType]
    public class UserData
    {
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [ReplicateIgnore]
        public Guid? Nonce { get; set; }
        public bool EmailVerified { get; set; }
        [ReplicateIgnore]
        public string Hash { get; set; }
        [ReplicateIgnore]
        public List<CommentData> Comments { get; set; } = new List<CommentData>();
        [ReplicateIgnore]
        public List<VoteData> Votes { get; set; } = new List<VoteData>();
        [ReplicateIgnore]
        public List<AttachmentData> Attachments { get; set; } = new List<AttachmentData>();
        [ReplicateIgnore]
        public List<UserBoardRole> Roles { get; set; } = new List<UserBoardRole>();
        [ReplicateIgnore]
        public List<CardData> CreatedCards { get; set; } = new List<CardData>();
        [ReplicateIgnore]
        public List<BoardData> CreatedBoards { get; set; } = new List<BoardData>();
    }
}
