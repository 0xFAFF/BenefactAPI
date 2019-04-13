using BenefactAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [ReplicateType]
    [ReplicateRoute(Route = "comments")]
    public class CommentsInterface
    {
        readonly IServiceProvider Services;
        public CommentsInterface(IServiceProvider services)
        {
            Services = services;
        }

        [AuthRequired(RequirePrivilege = Privileges.Comment)]
        public Task Add(CommentData comment)
        {
            return Services.DoWithDB(async db =>
            {
                comment.UserId = Auth.CurrentUser.Id;
                await db.Comments.AddAsync(comment);
                await db.SaveChangesAsync();
                return true;
            });
        }

        [AuthRequired(RequirePrivilege = Privileges.Modify)]
        public Task<bool> Update(CommentData comment)
        {
            return Services.DoWithDB(async db =>
            {
                var existingComment = await db.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id);
                if (existingComment == null) throw new HTTPError("Comment not found", 404);
                if (existingComment.UserId != Auth.CurrentUser.Id)
                    return false;
                existingComment.Text = comment.Text;
                existingComment.EditedTime = Util.Now();
                await db.SaveChangesAsync();
                return true;
            });
        }

        [AuthRequired(RequirePrivilege = Privileges.Modify)]
        public Task<bool> Delete(CommentData comment)
        {
            return Services.DoWithDB(async db =>
            {
                var existingComment = await db.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id);
                if (existingComment.UserId != Auth.CurrentUser.Id)
                    return false;
                if (await db.Delete(db.Comments, existingComment))
                {
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }, false);
        }
    }
}
