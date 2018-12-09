using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BenefactAPI.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Replicate;
using Replicate.MetaData;
using Replicate.Serialization;

namespace BenefactBackend.Controllers
{
    public class HTTPError : Exception
    {
        public int Status = 500;
        public HTTPError(string message, int status = 500) : base(message) { Status = status; }
    }
    public class HTTPChannel : ReplicationChannel<string, string>
    {
        public override IReplicateSerializer<string> Serializer { get; }
            = new JSONSerializer(ReplicationModel.Default) { ToLowerFieldNames = true };

        public HTTPChannel(TestImplentation implentation)
        {
            this.RegisterSingleton(implentation);
        }

        public override string GetEndpoint(MethodInfo endpoint)
        {
            return endpoint.Name.ToLower();
        }

        public override Task<object> Request(string messageID, RPCRequest request, ReliabilityMode reliability = ReliabilityMode.ReliableSequenced)
        {
            throw new NotImplementedException();
        }
    }
    public class TestImplentation : ICardsInterface
    {
        IServiceProvider Services;
        public TestImplentation(IServiceProvider services)
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
                    Cards = await db.Cards.Include(c => c.Categories).ToListAsync(),
                    Columns = await db.Columns.ToListAsync(),
                    Categories = await db.Categories.ToListAsync(),
                };
            });
        }
        public Task UpdateCard(CardData update)
        {
            return DoWithDB(async db =>
            {
                var existingCard = await db.Cards.Include(c => c.Categories).FirstOrDefaultAsync(c => c.Id == update.Id);
                if (existingCard == null) throw new HTTPError("Card not found");
                UpdateMembersFrom(existingCard, update, nameof(CardData.Id), nameof(CardData.CategoryIDs));
                if (update.CategoryIDs != null)
                {
                    existingCard.Categories.Clear();
                    existingCard.CategoryIDs = update.CategoryIDs;
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

        public Task<Category> AddCategory(Category category)
        {
            return DoWithDB(async db =>
            {
                var result = await db.Categories.AddAsync(category);
                await db.SaveChangesAsync();
                return result.Entity;
            });
        }

        public Task UpdateCategory(Category category)
        {
            return DoWithDB(async db =>
            {
                var existingCard = await db.Categories.FindAsync(category.Id);
                if (existingCard == null) throw new HTTPError("Category not found");
                UpdateMembersFrom(existingCard, category, nameof(Category.Id));
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
    [Route("api/")]
    public class ValuesController : Controller
    {

        public static ReplicationChannel<string, string> Channel;
        public static JSONSerializer serializer;
        IServiceProvider Provider;
        public ValuesController(HTTPChannel channel, IServiceProvider provider)
        {
            Channel = channel;
            Provider = provider;
        }
        // GET api/values
        [HttpGet("{*path}")]
        [HttpPost("{*path}")]
        public async Task<ActionResult> Post(string path)
        {
            try
            {
                using (var serviceScope = Provider.CreateScope())
                {
                    var bodyText = new StreamReader(Request.Body).ReadToEnd();
                    var result = await Channel.Receive(path, string.IsNullOrEmpty(bodyText) ? null : bodyText);
                    return new ContentResult() { Content = result, ContentType = "application/json", StatusCode = 200 };
                }
            }
            catch (SerializationError)
            {
                return new ContentResult() { Content = "Serialization error", ContentType = "text/plain", StatusCode = 500 };
            }
            catch (HTTPError httpError)
            {
                return new ContentResult() { Content = httpError.Message, ContentType = "text/plain", StatusCode = httpError.Status };
            }
            catch (ContractNotFoundError)
            {
                return new NotFoundResult();
            }
        }
    }
}
