using BenefactAPI.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BenefactAPI
{
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
        public static void UseBoards(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var boardId = context.GetRouteData()?.GetRouteParam("boardId", s => s);
                if (boardId != null)
                {
                    Board = await BoardLookup(context.RequestServices, boardId);
                    if (Auth.CurrentUser != null)
                        Auth.CurrentRole = Auth.CurrentUser.Roles.FirstOrDefault(ur => ur.BoardId == Board.Id);
                }
                await next();
            });
        }
        public static async Task<BoardData> BoardLookup(this IServiceProvider services, string urlName)
        {
            return await services.DoWithDB(db => db.Boards.FirstOrDefaultAsync(b => b.UrlName == urlName)) ?? throw new HTTPError("Board not found", 404);
        }
    }
}
