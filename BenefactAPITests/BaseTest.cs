using BenefactAPI;
using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Replicate.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenefactAPITests
{
    public class BaseTest
    {
        protected MockServiceProvider services;
        protected BoardController boardRPC;
        protected UserData user;
        [TestInitialize]
        public void Setup()
        {
            services = new MockServiceProvider();
            boardRPC = new BoardController(services);
            MockData.AddToDb(services);
        }
        [TestCleanup]
        public void Cleanup()
        {
            services.DoWithDB(db => db.Database.EnsureDeletedAsync()).GetAwaiter().GetResult();
        }
        public ControllerContext SetContext(string body, string board = "benefact")
        {
            RouteData routeData = new RouteData();
            routeData.Values["boardId"] = board;
            HttpContext httpContextMock = new DefaultHttpContext();
            var bytes = Encoding.UTF8.GetBytes(body);
            (httpContextMock.Request.Body = new MemoryStream()).Write(bytes, 0, bytes.Length);
            httpContextMock.Request.Body.Position = 0;
            return boardRPC.ControllerContext = new ControllerContext()
            {
                RouteData = routeData,
                HttpContext = httpContextMock,
            };
        }
        public async Task<UserData> GetUser(string email, Privilege? privilege)
        {
            return await services.DoWithDB(async db =>
            {
                user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
                var boards = await db.Boards.Include(b => b.Roles).ThenInclude(r => r.User).ToListAsync();
                foreach (var board in boards)
                {
                    var role = board.Roles.FirstOrDefault(r => r.User.Email == email);
                    if (role != null)
                    {
                        if (!privilege.HasValue)
                            await db.DeleteAsync(db.Roles, role);
                        else
                            role.Privilege = privilege.Value;
                    }
                    else if (privilege.HasValue)
                        await db.AddAsync(new UserRole()
                        {
                            BoardId = board.Id,
                            UserId = user.Id,
                            Privilege = privilege.Value
                        });
                }
                return user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            });
        }
        public async Task<string> Post(string url, string body, string board = "benefact")
        {
            SetContext(body);
            return ((ContentResult)(await boardRPC.Post(url))).Content;
        }
        public async Task<T> Post<T, U>(string url, U request, string board = "benefact")
        {
            return services.Serializer.Deserialize<T>(await Post(url, services.Serializer.Serialize(request).ReadAllString()));
        }
    }
}
