using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BenefactAPI.DataAccess;
using Microsoft.AspNetCore.Mvc;
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
        public override IReplicateSerializer<string> Serializer { get; } = new JSONSerializer(ReplicationModel.Default);

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
        public TestImplentation()
        {
        }
        private Dictionary<int, TagData> tags = new Dictionary<int, TagData>
        {
            { 1, new TagData() { Color = "#F00", ID = 1, Name = "Story"} },
            { 2, new TagData() { Color = "#0F0", ID = 2, Name = "Dev Task" } },
            { 3, new TagData() { Color = "#00F", ID = 3, Name = "Business Boiz" } },
            { 4, new TagData() { Character = "🐛", ID = 4, Name = "Bug" } },
        };
        private List<CardData> cards = new List<CardData>()
            {
                new CardData() { ID = 1, Description = "Some Markdown\n=====\n\n```csharp\n    var herp = \"derp\";\n```", Title = "Get MD Working", ColumnID = 2, Categories = new[] { 1,2,3,4 } },
                new CardData() { ID = 2, Description = "😈😈😈😈😈😈", Title = "Make sure UTF8 works 😑", ColumnID = 1, Categories = new[] { 1 } },
                new CardData() { ID = 3, Description = "There was a bug", Title = "Some Bug", ColumnID = 2, Categories = new[] { 4, 2 } },
                new CardData() { ID = 3, Description = "There was a bug", Title = "Fixed bug", ColumnID = 3, Categories = new[] { 4 } },
            };
        private List<ColumnData> statuses =>
            new[] { new ColumnData { ID = 1, Title = "To Do" }, new ColumnData { ID = 2, Title = "In Progress" }, new ColumnData { ID = 3, Title = "Done" } }.ToList();
        public async Task<CardsResponse> Cards()
        {
            return new CardsResponse()
            {
                Cards = await BenefactDB.DB.GetCards(),
                Columns = statuses,
                Tags = tags.Values.ToList(),
            };
        }
        public void Update(CardUpdate update)
        {
            var cardIndex = cards.FindIndex(card => card.ID == update.ID);
            if (cardIndex == -1) throw new HTTPError("Card not found");
            var updateCard = cards[cardIndex];
            var td = ReplicationModel.Default.GetTypeAccessor(typeof(CardData));
            if (update.CardFields != null)
                foreach (var member in td.MemberAccessors.Where(mem => mem.Info.Name != "ID"))
                {
                    var newValue = member.GetValue(update.CardFields);
                    if (newValue == null) continue;
                    member.SetValue(updateCard, newValue);
                }
            if (update.InsertAboveID.HasValue)
            {
                var destIndex = cards.FindIndex(card => card.ID == update.ID);
                if (destIndex == -1) throw new HTTPError("Destination card not found");
                cards.RemoveAt(cardIndex);
                cards.Insert(destIndex, updateCard);
            }
        }
    }
    [Route("api/")]
    public class ValuesController : Controller
    {

        public static ReplicationChannel<string, string> Channel;
        public static JSONSerializer serializer;
        static ValuesController()
        {
            Channel = new HTTPChannel();
            Channel.RegisterSingleton(new TestImplentation());
        }
        // GET api/values
        [HttpGet("{*path}")]
        [HttpPost("{*path}")]
        public async Task<ActionResult> Post(string path)
        {
            try
            {
                var bodyText = new StreamReader(Request.Body).ReadToEnd();
                var result = await Channel.Receive(path, string.IsNullOrEmpty(bodyText) ? null : bodyText);
                return new ContentResult() { Content = result, ContentType = "application/json", StatusCode = 200 };
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
