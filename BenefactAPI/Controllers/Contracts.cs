using Replicate;
using System;
using System.Collections.Generic;
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
        public int? Index { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [ReplicateIgnore]
        public ColumnData Column { get; set; }
        public int? ColumnId { get; set; }
        [ReplicateIgnore]
        public List<CardTag> Tags { get; set; } = new List<CardTag>();
        [NotMapped]
        public List<int> TagIds
        {
            get => Tags.Select(ccd => ccd.TagId).ToList();
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
    public class ColumnData
    {
        public int Id { get; set; }
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
        public List<CardData> Cards;
        public List<ColumnData> Columns;
        public List<TagData> Tags;
    }
    [ReplicateType(AutoMethods = AutoAdd.AllPublic)]
    public interface ICardsInterface
    {
        Task<CardsResponse> Cards();
        /// <summary>
        /// Update a card with the non-null fields provided in CardFields
        /// </summary>
        /// <param name="update"></param>
        Task UpdateCard(CardData update);
        Task<CardData> AddCard(CardData card);
        Task DeleteCard(int cardId);

        Task<TagData> AddTag(TagData tag);
        Task UpdateTag(TagData tag);
        Task DeleteTag(int tagId);

        Task<ColumnData> AddColumn(ColumnData column);
        Task UpdateColumn(ColumnData column);
        Task DeleteColumn(int columnId);
    }
}
