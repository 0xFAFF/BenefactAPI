using BenefactAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Replicate.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI
{
    public static class Util
    {
        public static void UpdateMembersFrom<T>(T target, T newFields, string[] whiteList = null, string[] blackList = null)
        {
            var td = ReplicationModel.Default.GetTypeAccessor(typeof(T));
            IEnumerable<MemberAccessor> members = td.MemberAccessors;
            if (whiteList != null && whiteList.Any())
                members = members.Where(mem => whiteList.Contains(mem.Info.Name));
            if (blackList != null && blackList.Any())
                members = members.Where(mem => !blackList.Contains(mem.Info.Name));
            foreach (var member in members)
            {
                var newValue = member.GetValue(newFields);
                if (newValue == null) continue;
                member.SetValue(target, newValue);
            }
        }
        public static double Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        public static Task<T> HandleDuplicate<T>(this Task<T> task, string key, string message)
        {
            return task.HandleError<T, DbUpdateException>(e =>
            {
                if (e.InnerException is PostgresException pe && pe.SqlState == "23505" && pe.ConstraintName == key)
                    throw new HTTPError(message);
            });
        }

        public static Task<T> HandleError<T, E>(this Task<T> task, Action<E> handler) where E : Exception
        {
            return task.ContinueWith(t =>
            {
                try
                {
                    return t.GetAwaiter().GetResult();
                }
                catch (E e)
                {
                    handler(e);
                    throw;
                }
            });
        }

        public static T GetRouteParam<T>(this ActionContext context, string key, Func<string, T> converter)
        {
            if (context.RouteData.Values.TryGetValue(key, out var value))
            {
                try
                {
                    return converter((string)value);
                }
                catch { }
            }
            throw new HTTPError($"Invalid URL param {key}");
        }
    }
}
