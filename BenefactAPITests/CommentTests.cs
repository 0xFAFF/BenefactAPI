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
    public class CommentTests : BaseTest
    {
        [TestMethod]
        public async Task HigherValueFails()
        {
            var cardId = (await services.DoWithDB(db => db.Cards.FirstAsync())).Id;
            user = Auth.CurrentUser = await GetUser("a@a.a", Privilege.Developer);
            SetContext($"{{\"CardId\": {cardId}, \"Text\": \"same user\"}}");
            var error = await Assert.ThrowsExceptionAsync<HTTPError>(
                () => boardRPC.Post("comments/add"));
        }
        [TestMethod]
        public async Task AdminSucceeds()
        {
            await services.DoWithDB(async db => (await db.Boards.Include(b => b.Roles).FirstOrDefaultAsync()).Roles[0].Privilege = Privilege.Admin);
            var cardId = (await services.DoWithDB(db => db.Cards.FirstAsync())).Id;
            var user = Auth.CurrentUser = await Auth.GetUser(services, "faff@faff.faff");
            var rpc = new BoardController(services);
            rpc.ControllerContext = SetContext($"{{\"CardId\": {cardId}, \"Text\": \"same user\"}}");
            var result = await rpc.Post("comments/add");
        }
        [TestMethod]
        public async Task CommentingSameUserSuccess()
        {
            var user = Auth.CurrentUser = await Auth.GetUser(services, "faff@faff.faff");
            var cardId = (await services.DoWithDB(db => db.Cards.FirstAsync())).Id;
            foreach (var role in user.Roles) role.Privilege = Privilege.None;
            var rpc = new BoardController(services);
            rpc.ControllerContext = SetContext($"{{\"CardId\": {cardId}, \"Text\": \"same user\"}}");
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
            rpc.ControllerContext = SetContext("{\"CardId\": 999999, \"Text\": \"This is a test commment!\"}");
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
            rpc.ControllerContext = SetContext($"{{\"CardId\": {cardId}, \"Text\": \"same user\"}}");
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
            rpc.ControllerContext = SetContext($"{{\"CardId\": {cardId}, \"Text\": \"different user\"}}");
            var result = await rpc.Post("comments/add");
            var card = await services.DoWithDB(db => db.Cards.Include(c => c.Comments).Where(c => c.Id == cardId).FirstOr404());
            Assert.AreEqual("different user", card.Comments.First().Text);
        }
    }
}
