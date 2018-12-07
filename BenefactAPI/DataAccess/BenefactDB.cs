using BenefactBackend.Controllers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Replicate;
using Replicate.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.DataAccess
{
    [ReplicateType]
    class TagMappingEntry
    {
        public int CardID;
        public int CategoryID;
    }
    public class BenefactDB
    {
        public static BenefactDB DB { get; } = new BenefactDB();
        string connString = "Host=localhost;Username=postgres;Database=benefact";

        public async Task<T> DoInTransaction<T>(Func<NpgsqlConnection, Task<T>> action, bool autoCommit = true)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                await conn.OpenAsync();
                var transaction = conn.BeginTransaction();
                try
                {
                    var result = await action(conn);
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        static object ConvertDBValueTo(object value, Type type)
        {
            if (value is DBNull)
                return null;
            return value;
        }

        public Task<List<T>> Query<T>(string query)
        {
            return DoInTransaction(async conn =>
            {
                List<T> results = new List<T>();
                var typeAcc = ReplicationModel.Default.GetTypeAccessor(typeof(T));
                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var result = Activator.CreateInstance<T>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var fieldname = reader.GetName(i);
                            var acc = typeAcc.MemberAccessors.FirstOrDefault(m => m.Info.Name.ToLower() == fieldname.ToLower());
                            if(acc != null)
                                acc.SetValue(result, ConvertDBValueTo(reader.GetValue(i), acc.Type));
                        }
                        results.Add(result);
                    }
                }
                return results;
            });
        }

        public async Task<List<CardData>> GetCards()
        {
            var tags = (await Query<TagMappingEntry>("select * from category_mapping"))
                .GroupBy(tag => tag.CardID)
                .ToDictionary(group => group.Key, group => group.Select(v => v.CategoryID).ToList());

            var cards = await Query<CardData>("select * from cards");
            foreach (var card in cards)
            {
                if (tags.TryGetValue(card.ID, out var cardTags))
                    card.Categories = cardTags;
                else
                    card.Categories = new int[] { };
            }
            return cards;
        }
    }
}
