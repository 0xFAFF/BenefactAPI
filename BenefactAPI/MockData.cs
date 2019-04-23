using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI
{
    public static class MockData
    {
        public static void AddToDb(IServiceProvider services)
        {
            var users = new UserInterface(services);
            users.Add(new UserCreateRequest()
            {
                Email = "faff@faff.faff",
                Name = "FAFF",
                Password = "fafffaff",
            }, false).GetAwaiter().GetResult();
            BoardData board1 = null;
            services.DoWithDB(async db =>
            {
                board1 = db.Boards.FirstOrDefault() ?? db.Boards.Add(new BoardData()).Entity;

                board1.Roles = new List<BoardRole>() {
                    new BoardRole() { BoardId = 1, Name = "Admin", Privilege = Privilege.Admin },
                };
                var board2 = db.Boards.Skip(1).FirstOrDefault() ?? db.Boards.Add(new BoardData()).Entity;

                var faff = await db.Users.FirstOrDefaultAsync(u => u.Name == "FAFF");
                faff.EmailVerified = true;
                faff.Roles.Add(new UserBoardRole()
                {
                    BoardId = 1,
                    UserId = faff.Id,
                    BoardRole = board1.Roles[0]
                });
                db.Tags.Add(new TagData()
                {
                    Name = "Story",
                    Color = "#001f3f",
                    BoardId = 1,
                });
                db.Tags.Add(new TagData()
                {
                    Name = "Dev Task",
                    Color = "#2ECC40",
                    BoardId = 1,
                });
                db.Tags.Add(new TagData()
                {
                    Name = "Business Boiz",
                    Color = "#FF851B",
                    BoardId = 1,
                });
                db.Tags.Add(new TagData()
                {
                    Name = "Bug",
                    Character = "bug",
                    BoardId = 1,
                });
                db.Tags.Add(new TagData()
                {
                    Name = "Star",
                    Color = "#F012BE",
                    Character = "star",
                    BoardId = 1,
                });
                db.SaveChanges();
                db.Columns.Add(new ColumnData()
                {
                    Title = "To Do",
                    Index = 1,
                    BoardId = 1,
                });
                db.Columns.Add(new ColumnData()
                {
                    Title = "In Progress",
                    Index = 2,
                    BoardId = 1,
                });
                db.Columns.Add(new ColumnData()
                {
                    Title = "Done",
                    Index = 3,
                    BoardId = 1,
                });
                db.SaveChanges();
                db.Cards.Add(new CardData()
                {
                    Title = "Get MD Working",
                    Description = "Some Markdown\n=====\n\n```csharp\n var herp = \"derp\";\n```",
                    ColumnId = 2,
                    TagIds = new[] { 1, 2, 3, 4, 5 }.ToList(),
                    BoardId = 1,
                    Index = 1,
                    AuthorId = 1,
                });
                db.Cards.Add(new CardData()
                {
                    Title = "Make sure UTF8 works 😑",
                    Description = "😈😈😈😈😈😈",
                    ColumnId = 1,
                    TagIds = new[] { 1 }.ToList(),
                    BoardId = 1,
                    Index = 2,
                    AuthorId = 1,
                });
                db.Cards.Add(new CardData()
                {
                    Title = "Some Bug",
                    Description = "There was a bug",
                    ColumnId = 2,
                    TagIds = new[] { 4, 2 }.ToList(),
                    BoardId = 1,
                    Index = 3,
                    AuthorId = 1,
                });
                db.Cards.Add(new CardData()
                {
                    Title = "Fixed Bug",
                    Description = "There was a bug",
                    ColumnId = 3,
                    TagIds = new[] { 4 }.ToList(),
                    BoardId = 1,
                    Index = 4,
                    AuthorId = 1,
                });
                db.SaveChanges();
                return true;
            }).GetAwaiter().GetResult();
        }
    }
}
