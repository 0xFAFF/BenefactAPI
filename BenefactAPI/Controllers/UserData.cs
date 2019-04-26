using Replicate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [ReplicateType]
    public class UserAuthRequest
    {
        public string Email;
        public string Password;
    }
    [ReplicateType]
    public class UserCreateRequest
    {
        public string Email;
        public string Name;
        public string Password;
    }
    [ReplicateType]
    public class UserVerificationRequest
    {
        public string Nonce;
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
    }
}
