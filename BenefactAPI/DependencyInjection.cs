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
        public static async Task<T> DoWithDB<T>(this IServiceProvider Services, Func<BenefactDbContext, Task<T>> func)
        {
            using (var scope = Services.CreateScope())
            using (var db = scope.ServiceProvider.GetService<BenefactDbContext>())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                var result = await func(db);
                transaction.Commit();
                await db.SaveChangesAsync();
                return result;
            }
        }
    }
}
