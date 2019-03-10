using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BenefactAPI.DataAccess;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
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
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UrlParam : Attribute
    {
        public string Name;
    }
    public class HTTPChannel : RPCChannel<string, string>
    {
        public override IReplicateSerializer<string> Serializer { get; }
            = new JSONGraphSerializer(new ReplicationModel() { DictionaryAsObject = true });

        public HTTPChannel(IServiceProvider services)
        {
            this.Respond<None, string>(Version);
            this.RegisterSingleton(new CardsInterface(services));
            this.RegisterSingleton(new UserInterface(services));
            this.RegisterSingleton(new CommentsInterface(services));
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
        public static AsyncLocal<HttpRequest> CurrentRequest = new AsyncLocal<HttpRequest>();
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
            CurrentRequest.Value = Request;
            try
            {
                using (var serviceScope = Provider.CreateScope())
                {
                    var bodyText = new StreamReader(Request.Body).ReadToEnd();
                    if (!Channel.TryGetContract(path, out var contract)) return new NotFoundResult();
                    if (contract.Method?.GetCustomAttribute<AuthRequiredAttribute>() != null)
                        await Auth.AuthorizeUser(Request, Provider);
                    var result = await Channel.Receive(path, string.IsNullOrEmpty(bodyText) ? null : bodyText);
                    Auth.CurrentUser.Value = null;
                    return new ContentResult() { Content = result, ContentType = "application/json", StatusCode = 200 };
                }
            }
            catch (ContractNotFoundError)
            {
                return new NotFoundResult();
            }
            finally
            {
                CurrentRequest.Value = null;
            }
        }
    }
}
