using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.DataAccess
{
    public enum Privilege
    {
        None = 0,
        Read = 1,
        Contribute = 2,
        Vote = 4,
        Comment = 8,
        Developer = 16,
        Admin = 128,
    }

    [ReplicateType]
    public class UserRole
    {
        public int UserId { get; set; }
        public UserData User { get; set; }
        [ReplicateIgnore]
        public int BoardId { get; set; }
        [ReplicateIgnore]
        public BoardData Board { get; set; }
        public Privilege Privilege { get; set; }
    }
}
