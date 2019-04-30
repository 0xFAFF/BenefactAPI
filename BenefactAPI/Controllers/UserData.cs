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
}
