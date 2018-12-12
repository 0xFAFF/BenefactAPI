using BenefactAPI.DataAccess;
using BenefactAPI.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Replicate.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    public class CardsInterface : ICardsInterface
    {
        IServiceProvider Services;
        public CardsInterface(IServiceProvider services)
        {
            Services = services;
        }
        void UpdateMembersFrom<T>(T target, T newFields, params string[] ignoredFields)
        {
            var td = ReplicationModel.Default.GetTypeAccessor(typeof(T));
            IEnumerable<MemberAccessor> members = td.MemberAccessors;
            if (ignoredFields != null)
                members = members.Where(mem => !ignoredFields.Contains(mem.Info.Name));
            foreach (var member in members)
            {
                var newValue = member.GetValue(newFields);
                if (newValue == null) continue;
                member.SetValue(target, newValue);
            }
        }
        public async Task<T> DoWithDB<T>(Func<BenefactDBContext, Task<T>> func)
        {
            using (var scope = Services.CreateScope())
            using (var db = scope.ServiceProvider.GetService<BenefactDBContext>())
            {
                return await func(db);
            }
        }
        public Task<CardsResponse> Cards()
        {
            return DoWithDB(async db =>
            {
                return new CardsResponse()
                {
                    Cards = await db.Cards.Include(c => c.Tags).ToListAsync(),
                    Columns = await db.Columns.ToListAsync(),
                    Tags = await db.Tags.ToListAsync(),
                };
            });
        }
        public Task UpdateCard(CardData update)
        {
            return DoWithDB(async db =>
            {
                var existingCard = await db.Cards.Include(c => c.Tags).FirstOrDefaultAsync(c => c.Id == update.Id);
                if (existingCard == null) throw new HTTPError("Card not found");
                UpdateMembersFrom(existingCard, update, nameof(CardData.Id), nameof(CardData.TagIds));
                if (update.TagIds != null)
                {
                    existingCard.Tags.Clear();
                    existingCard.TagIds = update.TagIds;
                }
                    // TODO: Ordering/index
                    await db.SaveChangesAsync();
                return true;
            });
        }

        public Task<CardData> AddCard(CardData card)
        {
            return DoWithDB(async db =>
            {
                var result = await db.Cards.AddAsync(card);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        public Task DeleteCard(int cardId)
        {
            return DoWithDB(async db =>
            {
                var result = db.Cards.Remove(new CardData() { Id = cardId });
                await db.SaveChangesAsync();
                return result;
            });
        }

        public Task<TagData> AddTag(TagData tag)
        {
            return DoWithDB(async db =>
            {
                var result = await db.Tags.AddAsync(tag);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        public Task DeleteTag(int tagId)
        {
            return DoWithDB(async db =>
            {
                var result = db.Tags.Remove(new TagData() { Id = tagId });
                await db.SaveChangesAsync();
                return result;
            });
        }

        public Task UpdateTag(TagData tag)
        {
            return DoWithDB(async db =>
            {
                var existingCard = await db.Tags.FindAsync(tag.Id);
                if (existingCard == null) throw new HTTPError("Tag not found");
                UpdateMembersFrom(existingCard, tag, nameof(TagData.Id));
                await db.SaveChangesAsync();
                return true;
            });
        }

        public Task<ColumnData> AddColumn(ColumnData column)
        {
            return DoWithDB(async db =>
            {
                var result = await db.Columns.AddAsync(column);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        public Task DeleteColumn(int columnId)
        {
            return DoWithDB(async db =>
            {
                var result = db.Columns.Remove(new ColumnData() { Id = columnId });
                await db.SaveChangesAsync();
                return result;
            });
        }

        public Task UpdateColumn(ColumnData column)
        {
            return DoWithDB(async db =>
            {
                var existingColumn = await db.Columns.FindAsync(column.Id);
                if (existingColumn == null) throw new HTTPError("Column not found");
                UpdateMembersFrom(existingColumn, column, nameof(ColumnData.Id));
                await db.SaveChangesAsync();
                return true;
            });
        }
    }
}
