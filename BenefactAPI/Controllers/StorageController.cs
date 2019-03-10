using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenefactAPI.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace BenefactAPI.Controllers
{
    public class StorageEntry
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }
        public string ContentType { get; set; }
    }

    [Route("api/files/")]
    public class StorageController : ControllerBase
    {
        IServiceProvider services;
        public StorageController(IServiceProvider services)
        {
            this.services = services;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int? id)
        {
            if (id == null) throw new HTTPError("Invalid file id");
            var file = await services.DoWithDB(db =>
               db.Files.FirstOrDefaultAsync(f => f.Id == id.Value)
            );
            if (file == null) return new NotFoundResult();

            return new FileContentResult(file.Data, new MediaTypeHeaderValue(file.ContentType));
        }
        [HttpPost("add")]
        public async Task<int> Post()
        {
            await Auth.AuthorizeUser(Request, services);
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null) throw new HTTPError("Post contains no files");
            var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var fileEntry = new StorageEntry()
            {
                Data = stream.ToArray(),
                ContentType = file.ContentType,
            };
            var id = (await services.DoWithDB(db => db.Files.AddAsync(fileEntry))).Entity.Id;
            Auth.CurrentUser.Value = null;
            return id;
        }
    }
}