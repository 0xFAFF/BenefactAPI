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
using Replicate;

namespace BenefactAPI.Controllers
{
    public class StorageEntry
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }
        public string ContentType { get; set; }
        public AttachmentData Attachment { get; set; }
    }
    public class AttachmentData
    {
        public int Id { get; set; }
        [ReplicateIgnore]
        [Required]
        public StorageEntry Storage { get; set; }
        public int StorageId { get; set; }
        [Required]
        [ReplicateIgnore]
        public CardData Card { get; set; }
        public int CardId { get; set; }
        [Required]
        [ReplicateIgnore]
        public UserData User { get; set; }
        public int UserId { get; set; }
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
        // TODO: Authz for board permissions
        [HttpPost("add")]
        public async Task<int> Post()
        {
            Auth.ThrowIfUnauthorized();
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null) throw new HTTPError("Post contains no files", 401);
            if (!Request.Form.TryGetValue("CardId", out var cardIdString) || !int.TryParse(cardIdString, out var cardId))
                throw new HTTPError("Invalid CardId", 401);
            var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var attachment = new AttachmentData()
            {
                Storage = new StorageEntry()
                {
                    Data = stream.ToArray(),
                    ContentType = file.ContentType,
                },
                CardId = cardId,
                UserId = Auth.CurrentUser.Id,
            };
            var id = (await services.DoWithDB(db => db.Attachments.AddAsync(attachment))).Entity.StorageId;
            return id;
        }
    }
}