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
        public int UserId;
        public string Nonce;
    }

    [Flags]
    public enum Privileges
    {
        None = 0,
        View = 1,
        Modify = 2,
        Vote = 4,
        Comment = 8,
        Invite = 16,
        Assignee = 32,
    }

    [ReplicateType]
    public class UserPrivilege
    {
        public int UserId { get; set; }
        [ReplicateIgnore]
        public UserData User { get; set; }

        public int BoardId { get; set; }
        [ReplicateIgnore]
        public BoardData Board { get; set; }

        public Privileges Privilege { get; set; }
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
        public List<CommentData> Comments { get; set; }
        [ReplicateIgnore]
        public List<VoteData> Votes { get; set; }
        [ReplicateIgnore]
        public List<AttachmentData> Attachments { get; set; }
        public List<UserPrivilege> Privileges { get; set; }
    }
}
