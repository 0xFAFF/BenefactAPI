using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BenefactAPI.Controllers
{
    [Route("api/storage/")]
    public class StorageController : Controller
    {
        [HttpGet("get/{guid}")]
        public async Task<ActionResult> Get(Guid guid)
        {
            if (guid == new Guid())
                throw new HTTPError("Invalid guid");
            return new ContentResult();
        }
    }
}