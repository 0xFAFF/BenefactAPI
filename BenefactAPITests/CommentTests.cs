using BenefactAPI;
using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BenefactAPITests
{
    [TestClass]
    public class CommentTests
    {
        MockServiceProvider services;
        [TestInitialize]
        public void Setup()
        {
            services = new MockServiceProvider();
            MockData.AddToDb(services);
        }
        [TestCleanup]
        public void Cleanup()
        {
            services.DoWithDB(db => db.Database.EnsureDeletedAsync()).GetAwaiter().GetResult();
        }
        static ControllerContext CreateContext(string body)
        {
            RouteData routeData = new RouteData();
            routeData.Values["boardId"] = "benefact";
            HttpContext httpContextMock = new DefaultHttpContext();
            var bytes = Encoding.UTF8.GetBytes(body);
            (httpContextMock.Request.Body = new MemoryStream()).Write(bytes, 0, bytes.Length);
            httpContextMock.Request.Body.Position = 0;
            return new ControllerContext()
            {
                RouteData = routeData,
                HttpContext = httpContextMock,
            };
        }
        [TestMethod]
        public async Task HigherValueFails()
        {
            await services.DoWithDB(async db => (await db.Boards.Include(b => b.Roles).FirstOrDefaultAsync()).Roles[0].Privilege = Privilege.Developer);
            var cardId = (await services.DoWithDB(db => db.Cards.FirstAsync())).Id;
            var user = Auth.CurrentUser = await Auth.GetUser(services, "a@a.a");
            var rpc = new BoardController(services);
            rpc.ControllerContext = CreateContext($"{{\"CardId\": {cardId}, \"Text\": \"same user\"}}");
            var error = await Assert.ThrowsExceptionAsync<HTTPError>(
                () => rpc.Post("comments/add"));
        }
        [TestMethod]
        public async Task AdminSucceeds()
        {
            await services.DoWithDB(async db => (await db.Boards.Include(b => b.Roles).FirstOrDefaultAsync()).Roles[0].Privilege = Privilege.Admin);
            var cardId = (await services.DoWithDB(db => db.Cards.FirstAsync())).Id;
            var user = Auth.CurrentUser = await Auth.GetUser(services, "faff@faff.faff");
            var rpc = new BoardController(services);
            rpc.ControllerContext = CreateContext($"{{\"CardId\": {cardId}, \"Text\": \"same user\"}}");
            var result = await rpc.Post("comments/add");
        }
        [TestMethod]
        public async Task CommentingSameUserSuccess()
        {
            var user = Auth.CurrentUser = await Auth.GetUser(services, "faff@faff.faff");
            var cardId = (await services.DoWithDB(db => db.Cards.FirstAsync())).Id;
            foreach (var role in user.Roles) role.Privilege = Privilege.None;
            var rpc = new BoardController(services);
            rpc.ControllerContext = CreateContext($"{{\"CardId\": {cardId}, \"Text\": \"same user\"}}");
            var result = await rpc.Post("comments/add");
            var card = await services.DoWithDB(db => db.Cards.Include(c => c.Comments).Where(c => c.Id == cardId).FirstOr404());
            Assert.AreEqual("same user", card.Comments.First().Text);
        }
        [TestMethod]
        public async Task CardNotFoundFail()
        {
            var user = Auth.CurrentUser = await Auth.GetUser(services, "a@a.a");
            foreach (var role in user.Roles) role.Privilege = Privilege.Contribute;
            var rpc = new BoardController(services);
            rpc.ControllerContext = CreateContext("{\"CardId\": 999999, \"Text\": \"This is a test commment!\"}");
            var error = await Assert.ThrowsExceptionAsync<HTTPError>(
                () => rpc.Post("comments/add"));
            Assert.AreEqual(404, error.Status);
        }
        [TestMethod]
        public async Task CommentingOtherUserFail()
        {
            var user = Auth.CurrentUser = await Auth.GetUser(services, "a@a.a");
            var cardId = (await services.DoWithDB(db => db.Cards.FirstAsync())).Id;
            foreach (var role in user.Roles) role.Privilege = Privilege.Contribute;
            var rpc = new BoardController(services);
            rpc.ControllerContext = CreateContext($"{{\"CardId\": {cardId}, \"Text\": \"same user\"}}");
            var error = await Assert.ThrowsExceptionAsync<HTTPError>(
                () => rpc.Post("comments/add"));
        }
        [TestMethod]
        public async Task CommentingOtherUserSuccess()
        {
            var cardId = (await services.DoWithDB(db => db.Cards.FirstAsync())).Id;
            var user = Auth.CurrentUser = await Auth.GetUser(services, "a@a.a");
            foreach (var role in user.Roles) role.Privilege = Privilege.Comment;
            var rpc = new BoardController(services);
            rpc.ControllerContext = CreateContext($"{{\"CardId\": {cardId}, \"Text\": \"different user\"}}");
            var result = await rpc.Post("comments/add");
            var card = await services.DoWithDB(db => db.Cards.Include(c => c.Comments).Where(c => c.Id == cardId).FirstOr404());
            Assert.AreEqual("different user", card.Comments.First().Text);
        }
    }
}
