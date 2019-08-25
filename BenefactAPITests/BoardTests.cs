using BenefactAPI;
using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using BenefactAPI.RPCInterfaces.Board;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Replicate;
using Replicate.Serialization;
using Replicate.Web;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BenefactAPITests
{
    [TestClass]
    public class BoardTests : BaseTest
    {
        [TestMethod]
        public async Task PrivateBoardShowsResult()
        {
            user = Auth.CurrentUser = await GetUser("a@a.a", null);
            var result = await Post<BoardResponse, None>("/", None.Value);
            Assert.AreEqual("benefact", result.UrlName);
            Assert.AreEqual(null, result.UserPrivilege);
            Assert.AreEqual(Privilege.None, result.DefaultPrivilege);
            Assert.IsNotNull(result.Description);
        }
        [TestMethod]
        public async Task PrivateBoardJoinFails()
        {
            user = Auth.CurrentUser = await GetUser("a@a.a", null);
            var error = await Assert.ThrowsExceptionAsync<HTTPError>(
                () => Post<UserRole, None>("join", None.Value));
        }
        [TestMethod]
        public async Task PublicBoardJoinSucceeds()
        {
            user = Auth.CurrentUser = await GetUser("a@a.a", null);
            await services.DoWithDB(async db =>
            {
                var board = await db.Boards.Where(b => b.UrlName == "benefact").FirstAsync();
                board.DefaultPrivilege = Privilege.Read;
            });
            var role = await Post<UserRole, None>("join", None.Value);
            user = Auth.CurrentUser = await Auth.GetUser(services, "a@a.a");
            Assert.AreEqual(Privilege.Read, role.Privilege);
            Assert.AreEqual(Privilege.Read, user.Roles.First(r => r.BoardId == role.BoardId).Privilege);
        }
        [TestMethod]
        public async Task CreateInviteReturnsExisting()
        {
            user = Auth.CurrentUser = await GetUser("a@a.a", Privilege.Admin);
            var firstResult = await Post<string, CreateInviteRequest>("invite",
                new CreateInviteRequest() { Privilege = (Privilege)7 });
            Assert.IsNotNull(firstResult);
            var secondResult = await Post<string, CreateInviteRequest>("invite",
                new CreateInviteRequest() { Privilege = (Privilege)7 });
            Assert.AreEqual(firstResult, secondResult);
        }
        [TestMethod]
        public async Task CreateInviteReturnsNew()
        {
            user = Auth.CurrentUser = await GetUser("a@a.a", Privilege.Admin);
            var firstResult = await Post<string, CreateInviteRequest>("invite",
                new CreateInviteRequest() { Privilege = (Privilege)7 });
            Assert.IsNotNull(firstResult);
            var secondResult = await Post<string, CreateInviteRequest>("invite",
                new CreateInviteRequest() { Privilege = (Privilege)3 });
            Assert.AreNotEqual(firstResult, secondResult);
        }
        [TestMethod]
        public async Task PrivateBoardJoinSucceeds()
        {
            user = Auth.CurrentUser = await GetUser("faff@faff.faff", Privilege.Admin);
            var firstResult = await Post<string, CreateInviteRequest>("invite",
                new CreateInviteRequest() { Privilege = (Privilege)7 });
            Assert.IsNotNull(firstResult);
            user = Auth.CurrentUser = await GetUser("a@a.a", null);
            var joinResult = await Post<UserRole, JoinRequest>("join", 
                new JoinRequest() { Key = firstResult });
            Assert.AreEqual(7, (int)joinResult.Privilege);
        }
    }
}
