using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BenefactAPI.DataAccess;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Replicate;
using Replicate.MetaData;
using Replicate.RPC;
using Replicate.Serialization;

namespace BenefactAPI.Controllers
{
    public class HTTPError : Exception
    {
        public int Status = 500;
        public HTTPError(string message, int status = 500) : base(message) { Status = status; }
    }
    public class HTTPChannel : RPCChannel<string, string>
    {
        public override IReplicateSerializer<string> Serializer { get; }
            = new JSONGraphSerializer(new ReplicationModel() { DictionaryAsObject = true });

        public HTTPChannel(CardsInterface cardsInterface, UserInterface userInterface)
        {
            this.RegisterSingleton(cardsInterface);
            this.RegisterSingleton(userInterface);
            this.Respond<None, string>(Version);
        }

        public Task<string> Version(None _)
        {
            return Task.FromResult(Environment.GetEnvironmentVariable("GIT_COMMIT"));
        }

        public override string GetEndpoint(MethodInfo endpoint) => endpoint.Name.ToLower();

        public override Task<string> Request(string messageID, RPCRequest request, ReliabilityMode reliability = ReliabilityMode.ReliableSequenced)
        {
            throw new NotImplementedException();
        }
    }
    [Route("api/")]
    public class ReplicateController : Controller
    {

        public static RPCChannel<string, string> Channel;
        IServiceProvider Provider;
        public ReplicateController(HTTPChannel channel, IServiceProvider provider)
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
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Headers", "*");
            Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            if (Request.Method == "OPTIONS")
            {
                var result = new ContentResult();
                return result;
            }
            try
            {
                using (var serviceScope = Provider.CreateScope())
                {
                    var bodyText = new StreamReader(Request.Body).ReadToEnd();
                    if (!Channel.TryGetContract(path, out var contract)) return new NotFoundResult();
                    if (contract.Method?.GetCustomAttribute<AuthRequiredAttribute>() != null)
                    {
                        var email = Auth.ValidateUserEmail(Request);
                        if (email == null) throw new HTTPError("Unauthorized", 401);
                        var user = Auth.CurrentUser.Value = await Provider.DoWithDB(async db => await db.Users.FirstOrDefaultAsync(u => u.Email == email));
                        if (user == null) throw new HTTPError("Unauthorized", 401);
                    }
                    var result = await Channel.Receive(path, string.IsNullOrEmpty(bodyText) ? null : bodyText);
                    Auth.CurrentUser.Value = null;
                    return new ContentResult() { Content = result, ContentType = "application/json", StatusCode = 200 };
                }
            }
            catch (ContractNotFoundError)
            {
                return new NotFoundResult();
            }
            catch (Exception exception)
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
