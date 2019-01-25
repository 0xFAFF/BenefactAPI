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
    /// <summary>
    /// All fields on this must have null defaults since it's used in CardUpdate
    /// and specifying a non-null default will make it clear fields!
    /// </summary>
    [ReplicateType]
    public class CardData : IOrdered
    {
        public int Id { get; set; }
        [Required]
        public int? Index { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
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
    }
    public class CardTag
    {
        public int CardId { get; set; }
        public CardData Card { get; set; }
        public int TagId { get; set; }
        public TagData Tag { get; set; }
    }
    [ReplicateType]
    public class ColumnData : IOrdered
    {
        public int Id { get; set; }
        public int? Index { get; set; }
        public string Title { get; set; }

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
    }
    [ReplicateType]
    public struct CardsResponse
    {
        public Dictionary<string, List<CardData>> Cards;
        public List<ColumnData> Columns;
        public List<TagData> Tags;
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
        public int? Limit;
    }
    [ReplicateType]
    public class CardQueryTerm
    {
        public List<int> Tags;
        public int? ColumnId;
        public string Title;
    }
    [ReplicateType(AutoMethods = AutoAdd.AllPublic)]
    public interface ICardsInterface
    {
        Task<CardsResponse> Cards(CardQuery query);
        /// <summary>
        /// Update a card with the non-null fields provided in CardFields
        /// </summary>
        /// <param name="update"></param>
        Task UpdateCard(CardData update);
        Task<CardData> AddCard(CardData card);
        Task<bool> DeleteCard(DeleteData card);

        Task<TagData> AddTag(TagData tag);
        Task UpdateTag(TagData tag);
        Task<bool> DeleteTag(DeleteData tag);

        Task<ColumnData> AddColumn(ColumnData column);
        Task UpdateColumn(ColumnData column);
        Task<bool> DeleteColumn(DeleteData column);
    }
}
