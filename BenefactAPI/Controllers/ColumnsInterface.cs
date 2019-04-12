using BenefactAPI.DataAccess;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
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

        [AuthRequired(RequirePrivilege = Privileges.Modify)]
        public Task<ColumnData> Add(ColumnData column)
        {
            return Services.DoWithDB(async db =>
            {
                column.Id = 0;
                column.BoardId = BoardController.Board.Id;
                var result = await db.Columns.AddAsync(column);
                await db.Insert(column, column.Index, db.Columns.BoardFilter());
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        [AuthRequired(RequirePrivilege = Privileges.Modify)]
        public Task<bool> Delete(DeleteData column)
        {
            return Services.DoWithDB(
                db => db.DeleteAndOrder(db.Columns, column.Id),
                false);
        }

        [AuthRequired(RequirePrivilege = Privileges.Modify)]
        public Task Update(ColumnData update)
        {
            return Services.DoWithDB(async db =>
            {
                var column = await db.Columns.FindAsync(BoardController.Board.Id, update.Id);
                if (column == null) throw new HTTPError("Column not found");
                Util.UpdateMembersFrom(column, update, whiteList: new[] { nameof(ColumnData.Title) });
                if (update.Index.HasValue)
                    await db.Insert(column, update.Index.Value, db.Columns.BoardFilter());
                await db.SaveChangesAsync();
                return true;
            });
        }
    }
}
