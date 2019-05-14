using BenefactAPI;
using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using BenefactAPI.RPCInterfaces.Board;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Replicate.Serialization;
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
            SetContext($"");
            var result = await Post<BoardResponse>("/");
            Assert.AreEqual("benefact", result.UrlName);
            Assert.AreEqual(null, result.UserRole);
            Assert.AreEqual(null, result.DefaultPrivilege);
            Assert.IsNotNull(result.Description);
        }
        [TestMethod]
        public async Task PrivateBoardJoinFails()
        {
            user = Auth.CurrentUser = await GetUser("a@a.a", null);
            SetContext($"");
            var error = await Assert.ThrowsExceptionAsync<HTTPError>(() => Post<UserRole>("join"));
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
            SetContext($"");
            var role = await Post<UserRole>("join");
            user = Auth.CurrentUser = await Auth.GetUser(services, "a@a.a");
            Assert.AreEqual(Privilege.Read, role.Privilege);
            Assert.AreEqual(Privilege.Read, user.Roles.First(r => r.BoardId == role.BoardId).Privilege);
        }
        [TestMethod]
        public async Task CreateInviteReturnsExisting()
        {
            user = Auth.CurrentUser = await GetUser("a@a.a", Privilege.Admin);
            SetContext("{\"Privilege\": 7}");
            var firstResult = await Post<string>("invite");
            Assert.IsNotNull(firstResult);
            SetContext("{\"Privilege\": 7}");
            var secondResult = await Post<string>("invite");
            Assert.AreEqual(firstResult, secondResult);
        }
        [TestMethod]
        public async Task CreateInviteReturnsNew()
        {
            user = Auth.CurrentUser = await GetUser("a@a.a", Privilege.Admin);
            SetContext("{\"Privilege\": 7}");
            var firstResult = await Post<string>("invite");
            Assert.IsNotNull(firstResult);
            SetContext("{\"Privilege\": 3}");
            var secondResult = await Post<string>("invite");
            Assert.IsNotNull(secondResult);
            Assert.AreNotEqual(firstResult, secondResult);
        }
        [TestMethod]
        public async Task PrivateBoardJoinSucceeds()
        {
            user = Auth.CurrentUser = await GetUser("faff@faff.faff", Privilege.Admin);
            SetContext("{\"Privilege\": 7}");
            var firstResult = await Post<string>("invite");
            Assert.IsNotNull(firstResult);
            user = Auth.CurrentUser = await GetUser("a@a.a", null);
            SetContext($"{{\"Key\": \"{firstResult}\"}}");
            var joinResult = await Post<UserRole>("join");
            Assert.AreEqual(7, (int)joinResult.Privilege);
        }
    }
}
