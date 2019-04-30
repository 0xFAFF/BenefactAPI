using BenefactAPI.DataAccess;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.RPCInterfaces
{
    [ReplicateType]
    public class TrelloCard
    {
        public string id;
        public bool closed;
        public List<string> idLabels;
        public List<TrelloAttachment> attachments;
        public string idList;
        public string desc;
        public string name;

        [ReplicateIgnore]
        public CardData Card;
    }
    [ReplicateType]
    public class TrelloAttachment
    {
        public string name;
        public string url;
        public List<TrelloAttachmentPreview> previews;
    }
    [ReplicateType]
    public class TrelloAttachmentPreview
    {
        public string url;
        public int width;
        public int height;
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
}
