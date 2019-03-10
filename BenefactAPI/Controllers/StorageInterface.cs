using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [ReplicateType(AutoMethods = AutoAdd.AllPublic)]
    public class StorageInterface
    {
        IServiceProvider Services;
        public StorageInterface(IServiceProvider services)
        {
            Services = services;
        }
    }
}
