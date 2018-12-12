using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Replicate;
using Replicate.MetaData;
using Replicate.Serialization;

namespace BenefactAPI.Controllers
{
    public class HTTPError : Exception
    {
        public int Status = 500;
        public HTTPError(string message, int status = 500) : base(message) { Status = status; }
    }
    public class HTTPChannel : ReplicationChannel<string, string>
    {
        public override IReplicateSerializer<string> Serializer { get; }
            = new JSONSerializer(ReplicationModel.Default);

        public HTTPChannel(CardsInterface implentation)
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
        [HttpOptions("{*path}")]
        public async Task<ActionResult> Post(string path)
        {
            if(Request.Method == "OPTIONS")
            {
                Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Response.Headers.Add("Access-Control-Allow-Headers", "*");
                Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                var result = new ContentResult();
                return result;
            }
            try
            {
                using (var serviceScope = Provider.CreateScope())
                {
                    var bodyText = new StreamReader(Request.Body).ReadToEnd();
                    var result = await Channel.Receive(path, string.IsNullOrEmpty(bodyText) ? null : bodyText);
                    return new ContentResult() { Content = result, ContentType = "application/json", StatusCode = 200 };
                }
            }
            catch (ContractNotFoundError)
            {
                return new NotFoundResult();
            }
            catch(Exception exception)
            {
                return FromException(exception);
            }
        }
        ContentResult FromException(Exception exception)
        {
            // TODO: Turn off stack traces in production probably eventually
            switch (exception)
            {
                case HTTPError httpError:
                    return new ContentResult() { Content = httpError.Message, ContentType = "text/plain", StatusCode = httpError.Status };
                default:
                    if (exception.InnerException != null)
                        return FromException(exception.InnerException);
                    return new ContentResult() { Content = exception.ToString(), ContentType = "text/plain", StatusCode = 500 };
            }
        }
    }
}
