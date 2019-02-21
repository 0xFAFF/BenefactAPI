using BenefactAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [ReplicateType(AutoMethods = AutoAdd.AllPublic)]
    public class CommentsInterface
    {
        IServiceProvider Services;
        public CommentsInterface(IServiceProvider services)
        {
            Services = services;
        }

        [AuthRequired]
        public Task AddComment(CommentData comment)
        {
            return Services.DoWithDB(async db =>
            {
                comment.UserId = Auth.CurrentUser.Value.Id;
                await db.Comments.AddAsync(comment);
                await db.SaveChangesAsync();
                return true;
            });
        }

        [AuthRequired]
        public Task<bool> UpdateComment(CommentData comment)
        {
            return Services.DoWithDB(async db =>
            {
                var existingComment = await db.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id);
                if (existingComment == null) throw new HTTPError("Card not found");
                if (existingComment.UserId != Auth.CurrentUser.Value.Id)
                    return false;
                existingComment.Text = comment.Text;
                existingComment.EditedTime = Util.Now();
                await db.SaveChangesAsync();
                return true;
            });
        }

        [AuthRequired]
        public Task<bool> DeleteComment(CommentData comment)
        {
            return Services.DoWithDB(async db =>
            {
                var existingComment = await db.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id);
                if (existingComment.UserId != Auth.CurrentUser.Value.Id)
                    return false;
                if (await db.Delete(db.Comments, existingComment))
                {
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            });
        }
    }
}
