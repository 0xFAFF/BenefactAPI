using BenefactAPI.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI
{
    public static class DependencyInjection
    {
        public static Task DoWithDB(this IServiceProvider Services, Func<BenefactDbContext, Task> func,
            bool autoSave = true)
        {
            return DoWithDB(Services, async db => { await func(db); return true; }, autoSave);
        }
        public static async Task<T> DoWithDB<T>(this IServiceProvider Services, Func<BenefactDbContext, Task<T>> func,
            bool autoSave = true)
        {
            using (var db = Services.CreateScope().ServiceProvider.GetService<BenefactDbContext>())
            //using (var transaction = await db.Database.BeginTransactionAsync())
            {
                var result = await func(db);
                if (autoSave)
                    await db.SaveChangesAsync();
                //transaction.Commit();
                return result;
            }
        }
    }
}
