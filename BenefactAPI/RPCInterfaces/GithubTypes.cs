using BenefactAPI.DataAccess;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.RPCInterfaces
{
    [ReplicateType]
    public class GithubCard
    {
        public string id;
        public string title;
        public string state;
        public string body;
        public List<GithubLabel> labels;


        [ReplicateIgnore]
        public CardData Card;
    }
    //[ReplicateType]
    //public class TrelloAttachment
    //{
    //    public string name;
    //    public string url;
    //    public List<TrelloAttachmentPreview> previews;
    //}
    //[ReplicateType]
    //public class TrelloAttachmentPreview
    //{
    //    public string url;
    //    public int width;
    //    public int height;
    //}
    //[ReplicateType]
    //public class TrelloList
    //{
    //    public string id;
    //    public string name;

    //    [ReplicateIgnore]
    //    public ColumnData Column;
    //}
    [ReplicateType]
    public class GithubLabel
    {
        public string id;
        public string name;
        public string color;

        [ReplicateIgnore]
        public TagData Tag;
    }
    public class GithubBoard
    {
        public Dictionary<string, GithubLabel> labels;
        public List<GithubCard> cards;

        public GithubBoard(List<GithubCard> cards)
        {
            this.cards = cards;
            labels = this.cards.SelectMany(c => c.labels).ToLookup(l => l.id).ToDictionary(l => l.Key, l => l.First());
        }
    }
}
