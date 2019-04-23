using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    public enum Privilege
    {
        None = 0,
        Read = 1,
        Contribute = 2,
        Developer = 3,
        Admin = 255,
    }

    [ReplicateType]
    public class BoardRole : IBoardId
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [ReplicateIgnore]
        public int BoardId { get; set; }
        [ReplicateIgnore]
        public BoardData Board { get; set; }
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
        public BoardRole BoardRole { get; set; }
    }
}
