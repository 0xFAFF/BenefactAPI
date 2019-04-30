using Replicate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.DataAccess
{
    public class StorageEntry
    {
        public int Id { get; set; }
        [Required]
        public byte[] Data { get; set; }
        [Required]
        public AttachmentData Attachment { get; set; }
    }
    [ReplicateType]
    public class AttachmentData : IBoardId, ICardReference
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
        public string Preview { get; set; }
        [ReplicateIgnore]
        public int BoardId { get; set; }
        [ReplicateIgnore]
        public BoardData Board { get; set; }
        [ReplicateIgnore]
        public StorageEntry Storage { get; set; }
        public int? StorageId { get; set; }
        [Required]
        [ReplicateIgnore]
        public CardData Card { get; set; }
        public int CardId { get; set; }
        [Required]
        [ReplicateIgnore]
        public UserData User { get; set; }
        public int UserId { get; set; }
    }
}
