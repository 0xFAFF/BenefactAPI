using BenefactAPI.Controllers;
using Replicate;
using Replicate.MetaData.Policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.DataAccess
{
    public enum ActivityType
    {
        Create,
        Update,
        Archive
    }

    [ReplicateType(AutoMembers = AutoAdd.None)]
    public class ActivityData : IBoardId
    {
        public int Id { get; set; }
        public UserData User { get; set; }
        [Replicate]
        public int UserId { get; set; }
        public BoardData Board { get; set; }
        [Replicate]
        public int BoardId { get; set; }

        [Replicate]
        public ActivityType Type { get; set; }
        [Replicate, SkipNull]
        public string Message { get; set; }
        [Replicate]
        public double Time { get; set; }

        public CardData Card { get; set; }
        [Replicate, SkipNull]
        public int? CardId { get; set; }
        public CommentData Comment { get; set; }
        [Replicate, SkipNull]
        public int? CommentId { get; set; }
    }

    public static class Activity
    {
        public static async Task<ActivityData> LogActivity(BenefactDbContext db, object entity,
            ActivityType type, string message = null)
        {
            var activity = new ActivityData()
            {
                BoardId = BoardExtensions.Board.Id,
                UserId = Auth.CurrentUser.Id,
                Type = type,
                Message = message,
                Time = Util.Now(),
            };

            if (entity is CardData card)
                activity.CardId = card.Id;
            else if (entity is CommentData comment)
            {
                activity.CommentId = comment.Id;
                activity.CardId = comment.CardId;
            }
            else throw new ArgumentException("Invalid type for entity", nameof(entity));

            activity = (await db.AddAsync(activity)).Entity;
            // TODO: Email about activity
            return activity;
        }
    }
}
