using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Replicate.MetaData;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BenefactAPI
{
    public static class Util
    {
        public static double Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        public static Task<T> HandleDuplicate<T>(this Task<T> task, string key, string message)
        {
            return task.HandleError<T, DbUpdateException>(e =>
            {
                if (e.InnerException is PostgresException pe && pe.SqlState == "23505" && pe.ConstraintName == key)
                    throw new HTTPError(message);
            });
        }

        public static async Task<T> HandleError<T, E>(this Task<T> task, Action<E> handler) where E : Exception
        {
            try
            {
                return await task;
            }
            catch (E e)
            {
                handler(e);
                throw;
            }
        }

        public static T GetRouteParam<T>(this RouteData routeData, string key, Func<string, T> converter)
        {
            if (routeData.Values.TryGetValue(key, out var value))
            {
                try
                {
                    return converter((string)value);
                }
                catch { }
            }
            return default;
        }

        public static void ConfigureKey<T>(this ModelBuilder modelBuilder,
            Expression<Func<BoardData, IEnumerable<T>>> boardProperty) where T : class, IBoardId
        {
            modelBuilder.Entity<T>().HasKey(c => new { c.BoardId, c.Id });
            modelBuilder.Entity<T>().Property(c => c.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<T>()
                .HasOne(e => e.Board)
                .WithMany(boardProperty)
                .HasForeignKey(e => e.BoardId);
        }

        public static void CardReference<T>(this ModelBuilder modelBuilder,
            Expression<Func<CardData, IEnumerable<T>>> cardProperty) where T : class, ICardReference
        {
            modelBuilder.Entity<CardData>()
                .HasMany(cardProperty)
                .WithOne(e => e.Card)
                .HasForeignKey(e => new { e.BoardId, e.CardId });
        }

        public static void DeleteData(this MigrationBuilder builder, string table)
        {
            builder.DeleteData(
                table: table,
                keyColumns: new string[] { },
                keyValues: new object[] { });
        }

        public static Task<T> FirstOr404<T>(this IQueryable<T> query, string name = null) where T : class
        {
            return query.FirstOrError((name ?? typeof(T).Name.Replace("Data", "")) + " not found", 404);
        }

        public static async Task<T> FirstOrError<T>(this IQueryable<T> query, string message, int status = 500) where T : class
        {
            return (await query.FirstOrDefaultAsync()) ??
                throw new HTTPError(message, status);
        }
    }
}
