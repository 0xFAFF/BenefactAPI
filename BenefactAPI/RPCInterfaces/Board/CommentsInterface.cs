using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using Replicate;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.RPCInterfaces.Board
{
    [ReplicateType]
    public class CommentRequest
    {
        public int CardId;
        public string Text;
    }
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
        public Task Add(CommentRequest request)
        {
            return Services.DoWithDB(async db =>
            {
                var comment = new CommentData();
                TypeUtil.CopyFrom(comment, request);
                var card = await db.Cards.BoardFilter(request.CardId).FirstOr404();
                if (card.AuthorId != Auth.CurrentUser.Id)
                    Auth.VerifyPrivilege(Privilege.Comment);
                comment.UserId = Auth.CurrentUser.Id;
                comment.BoardId = BoardExtensions.Board.Id;
                await db.Comments.AddAsync(comment);
                await Activity.LogActivity(db, comment, ActivityType.Create);
                return true;
            });
        }

        [AuthRequired]
        public Task<bool> Update(CommentData comment)
        {
            return Services.DoWithDB(async db =>
            {
                var existingComment = await db.Comments.BoardFilter(comment.Id).FirstOr404();
                if (existingComment.UserId != Auth.CurrentUser.Id)
                    Auth.VerifyPrivilege(Privilege.Developer);
                existingComment.Text = comment.Text;
                existingComment.EditedTime = Util.Now();
                await Activity.LogActivity(db, existingComment, ActivityType.Update);
                return true;
            });
        }

        [AuthRequired]
        public Task<bool> Delete(CommentData comment)
        {
            return Services.DoWithDB(async db =>
            {
                var existingComment = await db.Comments.BoardFilter(comment.Id).FirstOr404();
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
