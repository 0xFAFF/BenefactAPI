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
    public class BoardRole : IBoardId
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [ReplicateIgnore]
        public int BoardId { get; set; }
        public BoardData Board { get; set; }
        [ReplicateIgnore]
        public List<UserBoardRole> Users { get; set; }
        public Privilege Privilege { get; set; }
    }

    [ReplicateType]
    public class UserBoardRole
    {
        public int UserId { get; set; }
        public UserData User { get; set; }
        [ReplicateIgnore]
        public int BoardId { get; set; }
        [ReplicateIgnore]
        public BoardData Board { get; set; }
        [ReplicateIgnore]
        public int BoardRoleId { get; set; }
        public BoardRole BoardRole { get; set; }
    }
}
