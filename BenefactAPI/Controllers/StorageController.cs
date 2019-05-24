using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenefactAPI.DataAccess;
using BenefactAPI.RPCInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Replicate;

namespace BenefactAPI.Controllers
{
    [Route("api/board/{boardId}/files/")]
    [ReplicateRoute(Route = "files")]
    public class StorageController : ControllerBase
    {
        IServiceProvider Services;
        public StorageController(IServiceProvider services)
        {
            Services = services;
        }
        [HttpGet("{id}/{name}")]
        public async Task<ActionResult> Get(int? id, string boardId)
        {
            BoardExtensions.Board = await BoardExtensions.BoardLookup(Services, boardId);
            Auth.ThrowIfUnauthorized(privilege: Privilege.Read);
            if (id == null) throw new HTTPError("Invalid file id", 400);
            var attachment = (await Services.DoWithDB(db =>
               db.Attachments.Include(a => a.Storage).BoardFilter(id.Value).FirstOrDefaultAsync()
            ));
            if (attachment == null) throw new HTTPError("File not found", 404);

            return new FileContentResult(attachment.Storage.Data, new MediaTypeHeaderValue(attachment.ContentType));
        }
        [HttpPost("add")]
        public async Task<AttachmentData> Add(string boardId)
        {
            BoardExtensions.Board = await BoardExtensions.BoardLookup(Services, boardId);
            Auth.ThrowIfUnauthorized(privilege: Privilege.Contribute);
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null) throw new HTTPError("Post contains no files", 401);
            if (!Request.Form.TryGetValue("CardId", out var cardIdString) || !int.TryParse(cardIdString, out var cardId))
                throw new HTTPError("Invalid CardId", 401);
            var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var attachment = new AttachmentData()
            {
                BoardId = BoardExtensions.Board.Id,
                Storage = new StorageEntry()
                {
                    Data = stream.ToArray(),
                },
                Name = file.FileName,
                ContentType = file.ContentType,
                CardId = cardId,
                UserId = Auth.CurrentUser.Id,
            };
            return await Services.DoWithDB(async db =>
            {
                await db.Attachments.AddAsync(attachment);
                return attachment;
            });
        }
        public static Task<bool> Delete(IDRequest delete)
        {
            return ReplicateController.Services.DoWithDB(async db =>
            {
                var existing = await db.Attachments.Include(a => a.Storage).BoardFilter(delete.Id).FirstOrDefaultAsync();
                if (existing == null)
                    throw new HTTPError("Attachment not found", 404);
                if(existing.Storage != null)
                    await db.DeleteAsync(db.Files, existing.Storage);
                return await db.DeleteAsync(db.Attachments, existing);
            }, false);
        }
    }
}