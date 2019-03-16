using BenefactAPI.DataAccess;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [ReplicateType(AutoMethods = AutoAdd.AllPublic)]
    [ReplicateRoute(Route = "columns")]
    public class ColumnsInterface
    {
        readonly IServiceProvider Services;
        public ColumnsInterface(IServiceProvider services)
        {
            Services = services;
        }

        [AuthRequired]
        public Task<ColumnData> Add(ColumnData column)
        {
            return Services.DoWithDB(async db =>
            {
                column.Id = 0;
                var result = await db.Columns.AddAsync(column);
                await db.Insert(column, column.Index, db.Columns);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        [AuthRequired]
        public Task<bool> Delete(DeleteData column)
        {
            return Services.DoWithDB(async db =>
            {
                if (await db.Delete(db.Columns, new ColumnData() { Id = column.Id }))
                {
                    await db.Order(db.Columns);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }, false);
        }

        [AuthRequired]
        public Task Update(ColumnData update)
        {
            return Services.DoWithDB(async db =>
            {
                var column = await db.Columns.FindAsync(update.Id);
                if (column == null) throw new HTTPError("Column not found");
                Util.UpdateMembersFrom(column, update, whiteList: new[] { nameof(ColumnData.Title) });
                if (update.Index.HasValue)
                    await db.Insert(column, update.Index.Value, db.Columns);
                await db.SaveChangesAsync();
                return true;
            });
        }
    }
}
