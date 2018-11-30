using Replicate;
using System;
using System.Collections.Generic;
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
        public int ID;
        public string Title;
        public string Description;
        public int? ColumnID;
        public IEnumerable<int> Categories = null;
    }
    [ReplicateType]
    public class ColumnData
    {
        public int ID;
        public string Title;
    }
    [ReplicateType]
    public class TagData
    {
        public int ID;
        public string Name;
        public string Color;
        public string Character;
    }
    [ReplicateType]
    public struct CardsResponse
    {
        public List<CardData> Cards;
        public List<ColumnData> Columns;
        public List<TagData> Tags;
    }
    [ReplicateType]
    public struct CardUpdate
    {
        public int ID;
        public int? InsertAboveID;
        public CardData CardFields;
    }
    [ReplicateType(AutoMethods = AutoAdd.AllPublic)]
    public interface ICardsInterface
    {
        CardsResponse Cards();
        /// <summary>
        /// Update a card with the non-null fields provided in CardFields
        /// </summary>
        /// <param name="update"></param>
        void Update(CardUpdate update);
    }
}
