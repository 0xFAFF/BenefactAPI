using BenefactAPI.DataAccess;
using BenefactAPI.RPCInterfaces;
using BenefactAPI.RPCInterfaces.Board;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Replicate;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [Route("api/")]
    [ReplicateType]
    public class RootController : ReplicateController
    {
        public RootController(IServiceProvider provider) : base(provider)
        {
            Server.RegisterSingleton(new UserInterface(Services));
            Server.RegisterSingleton(new MetaInterface(Services));
            Server.Respond<None, string>(Version);
        }

        public Task<string> Version(None _)
        {
            return Task.FromResult(Environment.GetEnvironmentVariable("GIT_COMMIT"));
        }
    }
    [Route("api/boards/{boardId}")]
    [ReplicateType]
    public class BoardController : ReplicateController
    {
        public BoardController(IServiceProvider provider) : base(provider)
        {
            Server.RegisterSingleton(new CardsInterface(Services));
            Server.RegisterSingleton(new CommentsInterface(Services));
            Server.RegisterSingleton(new ColumnsInterface(Services));
            Server.RegisterSingleton(new TagsInterface(Services));
            Server.RegisterSingleton(new BoardsInterface(Services));
            Server.Respond<IDRequest, bool>(StorageController.Delete);
        }
        public override async Task<ActionResult> Handle(string path)
        {
            BoardExtensions.Board = await BoardExtensions.BoardLookup(Services, ControllerContext.GetRouteParam("boardId", s => s));
            if (Auth.CurrentUser != null)
                Auth.CurrentRole = Auth.CurrentUser.Roles.FirstOrDefault(ur => ur.BoardId == BoardExtensions.Board.Id);
            return await base.Handle(path);
        }
    }
    public static class BoardExtensions
    {
        private static AsyncLocal<BoardData> _currentBoard = new AsyncLocal<BoardData>();
        public static BoardData Board { get => _currentBoard.Value; set => _currentBoard.Value = value; }
        public static IQueryable<T> BoardFilter<T>(this IQueryable<T> set, int? setId = null) where T : class, IBoardId
        {
            var filter = set.Where(e => e.BoardId == Board.Id);
            if (setId.HasValue)
                filter = filter.Where(e => e.Id == setId.Value);
            return filter;
        }
        public static async Task<BoardData> BoardLookup(this IServiceProvider services, string urlName)
        {
            return await services.DoWithDB(db => db.Boards.FirstOrDefaultAsync(b => b.UrlName == urlName)) ?? throw new HTTPError("Board not found", 404);
        }
    }
}
