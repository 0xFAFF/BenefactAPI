using BenefactAPI.DataAccess;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [ReplicateType(AutoMethods = AutoAdd.AllPublic)]
    [ReplicateRoute(Route = "tags")]
    public class TagsInterface
    {
        readonly IServiceProvider Services;
        public TagsInterface(IServiceProvider services)
        {
            Services = services;
        }

        [AuthRequired]
        public Task<TagData> Add(TagData tag)
        {
            return Services.DoWithDB(async db =>
            {
                tag.Id = 0;
                var result = await db.Tags.AddAsync(tag);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        [AuthRequired]
        public Task<bool> Delete(DeleteData tag)
        {

            return Services.DoWithDB(
                db => db.Delete(db.Tags, new TagData() { Id = tag.Id }),
                false);
        }

        [AuthRequired]
        public Task Update(TagData tag)
        {
            return Services.DoWithDB(async db =>
            {
                var existingCard = await db.Tags.FindAsync(tag.Id);
                if (existingCard == null) throw new HTTPError("Tag not found");
                Util.UpdateMembersFrom(existingCard, tag, blackList: new[] { nameof(TagData.Id) });
                await db.SaveChangesAsync();
                return true;
            });
        }
    }
}
