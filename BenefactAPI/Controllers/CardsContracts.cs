using Replicate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    public interface IOrdered
    {
        int? Index { get; set; }
    }

    public interface IId
    {
        int Id { get; }
    }

    [ReplicateType]
    public class BoardData
    {
        public int Id { get; set; }
        public List<CardData> Cards { get; set; }
        public List<ColumnData> Columns { get; set; }
        public List<TagData> Tags { get; set; }
    }

    /// <summary>
    /// All fields on this must have null defaults since it's used in CardUpdate
    /// and specifying a non-null default will make it clear fields!
    /// </summary>
    [ReplicateType]
    public class CardData : IOrdered, IId
    {
        public int Id { get; set; }
        [Required]
        public int? Index { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [ReplicateIgnore]
        public BoardData Board { get; set; }
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
        [ReplicateIgnore]
        public List<AttachmentData> Attachments { get; set; } = new List<AttachmentData>();
        [NotMapped]
        public IEnumerable<int> AttachmentIds { get => Attachments.Select(a => a.StorageId); /*TODO: Remove this*/set => value = null; }
    }
    [ReplicateType]
    public class CommentData
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int UserId { get; set; }
        [Required]
        [ReplicateIgnore]
        public UserData User { get; set; }
        public int CardId { get; set; }
        [Required]
        [ReplicateIgnore]
        public CardData Card { get; set; }
        public double CreatedTime { get; set; } = Util.Now();
        public double? EditedTime { get; set; }
    }
    [ReplicateType]
    public class CardVoteRequest
    {
        public int Count;
        public int CardId;
    }
    [ReplicateType]
    public class VoteData
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
    }
    public class CardTag
    {
        public int CardId { get; set; }
        public CardData Card { get; set; }
        public int TagId { get; set; }
        public TagData Tag { get; set; }
    }
    [ReplicateType]
    public class ColumnData : IOrdered, IId
    {
        public int Id { get; set; }
        public int? Index { get; set; }
        public string Title { get; set; }

        [ReplicateIgnore]
        public BoardData Board { get; set; }
        public int BoardId { get; set; }

        [ReplicateIgnore]
        public List<CardData> Cards { get; set; }
    }
    [ReplicateType]
    public class TagData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Character { get; set; }

        [ReplicateIgnore]
        public BoardData Board { get; set; }
        public int BoardId { get; set; }
    }
    [ReplicateType]
    public struct CardsResponse
    {
        public Dictionary<string, List<CardData>> Cards;
        public List<ColumnData> Columns;
        public List<TagData> Tags;
        public List<UserData> Users;
    }
    [ReplicateType]
    public class DeleteData
    {
        public int Id { get; set; }
    }
    [ReplicateType]
    public class CardQuery
    {
        public Dictionary<string, List<CardQueryTerm>> Groups;
        public int BoardId;
        public int? Limit;
    }
    [ReplicateType]
    public class CardQueryTerm
    {
        public List<int> Tags;
        public int? ColumnId;
        public string Title;
    }
}
