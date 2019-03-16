using Microsoft.AspNetCore.Mvc;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [Route("api/")]
    public class RootController : ReplicateController
    {
        public RootController(IServiceProvider provider) : base(provider)
        {
        }
    }
}
