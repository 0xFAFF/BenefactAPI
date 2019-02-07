using Replicate;
using System;
using System.Collections.Generic;
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
    public class UserData
    {
        public int Id { get; set; }
        public string Email { get; set; }
        [ReplicateIgnore]
        public string Hash { get; set; }
    }
}
