using BenefactAPI.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [Route("api/")]
    public class RootController : ReplicateController
    {
        public RootController(IServiceProvider provider) : base(provider)
        {
            Channel.RegisterSingleton(new UserInterface(Services));
            Channel.Respond<None, string>(Version);
        }

        public Task<string> Version(None _)
        {
            return Task.FromResult(Environment.GetEnvironmentVariable("GIT_COMMIT"));
        }
    }
    [Route("api/board/{boardId:int}")]
    public class BoardController : ReplicateController
    {
        public BoardController(IServiceProvider provider) : base(provider)
        {
            Channel.RegisterSingleton(new CardsInterface(Services));
            Channel.RegisterSingleton(new CommentsInterface(Services));
            Channel.RegisterSingleton(new ColumnsInterface(Services));
            Channel.RegisterSingleton(new TagsInterface(Services));
            Channel.RegisterSingleton(new BoardsInterface(Services));
            Channel.Respond<DeleteData, bool>(StorageController.Delete);
        }
        public override async Task<ActionResult> Handle(string path)
        {
            BoardExtensions.Board = await BoardExtensions.BoardLookup(Services, ControllerContext.GetRouteParam("boardId", int.Parse));
            if (Auth.CurrentUser != null)
                Auth.CurrentRole = Auth.CurrentUser.Roles.FirstOrDefault(ur => ur.BoardId == BoardExtensions.Board.Id)?.BoardRole;
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
        public static async Task<BoardData> BoardLookup(this IServiceProvider services, int boardId)
        {
            return await services.DoWithDB(db => db.Boards.FirstOrDefaultAsync(b => b.Id == boardId)) ?? throw new HTTPError("Board not found", 404);
        }
    }
}
