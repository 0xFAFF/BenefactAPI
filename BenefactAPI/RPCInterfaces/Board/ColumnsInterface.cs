using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using Replicate;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.RPCInterfaces.Board
{
    [ReplicateType]
    [ReplicateRoute(Route = "columns")]
    public class ColumnsInterface
    {
        readonly IServiceProvider Services;
        public ColumnsInterface(IServiceProvider services)
        {
            Services = services;
        }

        [AuthRequired(RequirePrivilege = Privilege.Admin)]
        public Task<ColumnData> Add(ColumnData column)
        {
            return Services.DoWithDB(async db =>
            {
                column.Id = 0;
                column.BoardId = BoardExtensions.Board.Id;
                var result = await db.Columns.AddAsync(column);
                await db.Insert(column, column.Index, db.Columns.BoardFilter());
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        [AuthRequired(RequirePrivilege = Privilege.Admin)]
        public Task<bool> Delete(IDRequest column)
        {
            return Services.DoWithDB(
                db => db.DeleteOrderAsync(db.Columns, column.Id),
                false);
        }

        [AuthRequired(RequirePrivilege = Privilege.Admin)]
        public Task Update(ColumnData update)
        {
            return Services.DoWithDB(async db =>
            {
                var column = await db.Columns.FindAsync(BoardExtensions.Board.Id, update.Id);
                if (column == null) throw new HTTPError("Column not found", 404);
                TypeUtil.CopyFrom(column, update, whiteList: new[] {
                    nameof(ColumnData.Title), nameof(ColumnData.State),
                    nameof(ColumnData.AllowContribution) });
                if (update.Index.HasValue)
                    await db.Insert(column, update.Index.Value, db.Columns.BoardFilter());
                await db.SaveChangesAsync();
                return true;
            });
        }
    }
}
