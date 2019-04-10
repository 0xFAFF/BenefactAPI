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
        private static AsyncLocal<BoardData> _currentBoard = new AsyncLocal<BoardData>();
        public static BoardData CurrentBoard => _currentBoard.Value;
        public BoardController(IServiceProvider provider) : base(provider)
        {
            Channel.RegisterSingleton(new CardsInterface(Services));
            Channel.RegisterSingleton(new CommentsInterface(Services));
            Channel.RegisterSingleton(new ColumnsInterface(Services));
            Channel.RegisterSingleton(new TagsInterface(Services));
        }
        public override async Task<ActionResult> Handle(string path)
        {
            _currentBoard.Value = await Services.DoWithDB(db => db.Boards.FirstOrDefaultAsync(b => b.Id == ControllerContext.GetRouteParam("boardId", int.Parse)));
            return await base.Handle(path);
        }
    }
}
