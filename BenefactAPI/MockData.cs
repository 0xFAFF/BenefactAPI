using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using BenefactAPI.RPCInterfaces;
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
            users.Add(new UserCreateRequest()
            {
                Email = "a@a.a",
                Name = "A",
                Password = "a",
            }, false).GetAwaiter().GetResult();
            BoardData board1 = null;
            services.DoWithDB(async db =>
            {
                board1 = db.Boards.FirstOrDefault() ?? db.Boards.Add(new BoardData()
                {
                    Id = 1,
                    Title = "Benefact",
                    UrlName = "benefact",
                    Description = "Benefact board",
                }).Entity;

                var board2 = db.Boards.Skip(1).FirstOrDefault() ?? db.Boards.Add(new BoardData() { Id = 2, Title = "FAFF", UrlName = "faff" }).Entity;

                var faff = await db.Users.FirstOrDefaultAsync(u => u.Name == "FAFF");
                var aUser = await db.Users.FirstOrDefaultAsync(u => u.Name == "A");
                aUser.EmailVerified = true;
                faff.EmailVerified = true;
                faff.Roles.Add(new UserRole()
                {
                    BoardId = 1,
                    UserId = faff.Id,
                    Privilege = Privilege.Admin,
                });
                aUser.Roles.Add(new UserRole()
                {
                    BoardId = 1,
                    UserId = aUser.Id,
                    Privilege = Privilege.Read,
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
                var todo = db.Columns.Add(new ColumnData()
                {
                    State = CardState.Proposed,
                    AllowContribution = false,
                    Title = "To Do",
                    Index = 1,
                    BoardId = 1,
                }).Entity;
                var inProgress = db.Columns.Add(new ColumnData()
                {
                    AllowContribution = false,
                    Title = "In Progress",
                    State = CardState.InProgress,
                    Index = 2,
                    BoardId = 1,
                }).Entity;
                var done = db.Columns.Add(new ColumnData()
                {
                    AllowContribution = false,
                    Title = "Done",
                    State = CardState.Complete,
                    Index = 3,
                    BoardId = 1,
                }).Entity;
                db.SaveChanges();
                db.Cards.Add(new CardData()
                {
                    Title = "Get MD Working",
                    Description = "Some Markdown\n=====\n\n```csharp\n var herp = \"derp\";\n```",
                    ColumnId = todo.Id,
                    TagIds = new[] { 1, 2, 3, 4, 5 }.ToList(),
                    BoardId = 1,
                    Index = 1,
                    AuthorId = faff.Id,
                    Archived = false,
                });
                db.Cards.Add(new CardData()
                {
                    Title = "Make sure UTF8 works 😑",
                    Description = "😈😈😈😈😈😈",
                    ColumnId = todo.Id,
                    TagIds = new[] { 1 }.ToList(),
                    BoardId = 1,
                    Index = 2,
                    AuthorId = faff.Id,
                    Archived = false,
                });
                db.Cards.Add(new CardData()
                {
                    Title = "Some Bug",
                    Description = "There was a bug",
                    ColumnId = inProgress.Id,
                    TagIds = new[] { 4, 2 }.ToList(),
                    BoardId = 1,
                    Index = 3,
                    AuthorId = faff.Id,
                    Archived = false,
                });
                db.Cards.Add(new CardData()
                {
                    Title = "Fixed Bug",
                    Description = "There was a bug",
                    ColumnId = done.Id,
                    TagIds = new[] { 4 }.ToList(),
                    BoardId = 1,
                    Index = 4,
                    AuthorId = faff.Id,
                    Archived = false,
                });
                db.SaveChanges();
                return true;
            }).GetAwaiter().GetResult();
        }
    }
}
