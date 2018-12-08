using Replicate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactBackend.Controllers
{
    /// <summary>
    /// All fields on this must have null defaults since it's used in CardUpdate
    /// and specifying a non-null default will make it clear fields!
    /// </summary>
    [ReplicateType]
    public class CardData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? ColumnID { get; set; }
        [ReplicateIgnore]
        public ColumnData Column { get; set; }
        [ReplicateIgnore]
        public List<CardCategory> Categories { get; set; } = new List<CardCategory>();
        [NotMapped]
        public List<int> CategoryIDs
        {
            get => Categories.Select(ccd => ccd.CategoryId).ToList();
            set => Categories = value.Select(v => new CardCategory() { Card = this, CategoryId = v }).ToList();
        }
    }
    public class CardCategory
    {
        public int CardId { get; set; }
        public CardData Card { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }

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
    public class Category
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
        public List<Category> Categories;
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

        Task<Category> AddCategory(Category category);
        Task UpdateCategory(Category category);

        Task<ColumnData> AddColumn(ColumnData column);
        Task UpdateColumn(ColumnData column);
    }
}
