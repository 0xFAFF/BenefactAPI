using BenefactAPI.DataAccess;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [ReplicateType]
    [ReplicateRoute(Route = "tags")]
    public class TagsInterface
    {
        readonly IServiceProvider Services;
        public TagsInterface(IServiceProvider services)
        {
            Services = services;
        }

        [AuthRequired(RequirePrivilege = Privileges.Modify)]
        public Task<TagData> Add(TagData tag)
        {
            return Services.DoWithDB(async db =>
            {
                tag.Id = 0;
                tag.BoardId = BoardExtensions.Board.Id;
                var result = await db.Tags.AddAsync(tag);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        [AuthRequired(RequirePrivilege = Privileges.Modify)]
        public Task<bool> Delete(DeleteData tag)
        {

            return Services.DoWithDB(
                db => db.Delete(db.Tags, new TagData() { Id = tag.Id, BoardId = BoardExtensions.Board.Id }),
                false);
        }

        [AuthRequired(RequirePrivilege = Privileges.Modify)]
        public Task Update(TagData tag)
        {
            return Services.DoWithDB(async db =>
            {
                var existingTag = await db.Tags.FindAsync(BoardExtensions.Board.Id, tag.Id);
                if (existingTag == null) throw new HTTPError("Tag not found", 404);
                Util.UpdateMembersFrom(existingTag, tag, whiteList: new[] { nameof(TagData.Name), nameof(TagData.Character), nameof(TagData.Color) });
                await db.SaveChangesAsync();
                return true;
            });
        }
    }
}
