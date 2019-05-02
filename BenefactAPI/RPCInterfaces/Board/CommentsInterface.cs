using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.RPCInterfaces.Board
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

        [AuthRequired]
        public Task Add(CommentData comment)
        {
            return Services.DoWithDB(async db =>
            {
                comment.UserId = Auth.CurrentUser.Id;
                comment.BoardId = BoardExtensions.Board.Id;
                await db.Comments.AddAsync(comment);
                await db.SaveChangesAsync();
                return true;
            });
        }

        [AuthRequired]
        public Task<bool> Update(CommentData comment)
        {
            return Services.DoWithDB(async db =>
            {
                var existingComment = await db.Comments.BoardFilter(comment.Id).FirstOrDefaultAsync();
                if (existingComment == null) throw new HTTPError("Comment not found", 404);
                if (existingComment.UserId != Auth.CurrentUser.Id)
                    Auth.VerifyPrivilege(Privilege.Developer);
                existingComment.Text = comment.Text;
                existingComment.EditedTime = Util.Now();
                await db.SaveChangesAsync();
                return true;
            });
        }

        [AuthRequired]
        public Task<bool> Delete(CommentData comment)
        {
            return Services.DoWithDB(async db =>
            {
                var existingComment = await db.Comments.BoardFilter(comment.Id).FirstOrDefaultAsync();
                if (existingComment.UserId != Auth.CurrentUser.Id)
                    Auth.VerifyPrivilege(Privilege.Developer);
                if (await db.DeleteAsync(db.Comments, existingComment))
                {
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }, false);
        }
    }
}
