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
    public class AuthorizationTests
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
            var user = Auth.CurrentUser = await Auth.GetUser(services, "faff@faff.faff");
            var rpc = new RootController(services);
            rpc.ControllerContext = CreateContext("{\"CardId\": 1, \"Text\": \"This is a test commment!\"}");
            var error = await Assert.ThrowsExceptionAsync<HTTPError>(
                () => rpc.Post("comments/add"));
        }
        [TestMethod]
        public async Task AdminSucceeds()
        {
            await services.DoWithDB(async db => (await db.Boards.Include(b => b.Roles).FirstOrDefaultAsync()).Roles[0].Privilege = Privilege.Admin);
            var user = Auth.CurrentUser = await Auth.GetUser(services, "faff@faff.faff");
            var rpc = new RootController(services);
            rpc.ControllerContext = CreateContext("{\"CardId\": 1, \"Text\": \"This is a test commment!\"}");
            var result = rpc.Post("comments/add");
        }
        [TestMethod]
        public async Task CommentingFail()
        {
            await services.DoWithDB(async db => (await db.Boards.Include(b => b.Roles).FirstOrDefaultAsync()).Roles[0].Privilege = Privilege.Contribute);
            var user = Auth.CurrentUser = await Auth.GetUser(services, "faff@faff.faff");
            var rpc = new RootController(services);
            rpc.ControllerContext = CreateContext("{\"CardId\": 1, \"Text\": \"This is a test commment!\"}");
            var error = await Assert.ThrowsExceptionAsync<HTTPError>(
                () => rpc.Post("comments/add"));
        }
        [TestMethod]
        public async Task CommentingSuccess()
        {
            await services.DoWithDB(async db => (await db.Boards.Include(b => b.Roles).FirstOrDefaultAsync()).Roles[0].Privilege = Privilege.Comment);
            var user = Auth.CurrentUser = await Auth.GetUser(services, "faff@faff.faff");
            var rpc = new RootController(services);
            rpc.ControllerContext = CreateContext("{\"CardId\": 1, \"Text\": \"This is a test commment!\"}");
            var result = await rpc.Post("comments/add");
        }
    }
}
