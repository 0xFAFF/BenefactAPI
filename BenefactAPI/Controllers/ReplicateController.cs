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
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Replicate;
using Replicate.MetaData;
using Replicate.RPC;
using Replicate.Serialization;

namespace BenefactAPI.Controllers
{
    public class ReplicateRouteAttribute : Attribute
    {
        public string Route;
    }
    public class HTTPError : Exception
    {
        public int Status = 500;
        public HTTPError(string message, int status = 500) : base(message) { Status = status; }
    }
    public class HTTPChannel : RPCChannel<string, string>
    {
        public HTTPChannel(IReplicateSerializer<string> serializer) : base(serializer) { }

        public override string GetEndpoint(MethodInfo endpoint)
        {
            var methodRoute = endpoint.GetCustomAttribute<ReplicateRouteAttribute>();
            var name = methodRoute?.Route ?? endpoint.Name.ToLower();
            var classRoute = endpoint.DeclaringType.GetCustomAttribute<ReplicateRouteAttribute>();
            if (classRoute != null)
                name = $"{classRoute.Route}/{name}";
            while (name.Any() && name.Last() == '/')
                name = name.Substring(0, name.Length - 1);
            return name;
        }

        public override Task<string> Request(string messageID, RPCRequest request, ReliabilityMode reliability = ReliabilityMode.ReliableSequenced)
        {
            throw new NotImplementedException();
        }
    }
    public class ReplicateController : Controller
    {
        private static AsyncLocal<ActionContext> context = new AsyncLocal<ActionContext>();
        public static ActionContext CurrentContext => context.Value;

        public readonly RPCChannel<string, string> Channel;
        protected IServiceProvider Services;
        readonly ILogger logger;
        public ReplicateController(IServiceProvider services)
        {
            Channel = services.GetRequiredService<HTTPChannel>();
            logger = services.GetService<ILogger<RootController>>();
            Services = services;
        }

        // GET api/values
        [HttpGet("{*path}")]
        [HttpPost("{*path}")]
        [HttpOptions("{*path}")]
        public virtual async Task<ActionResult> Post(string path)
        {
            context.Value = ControllerContext;
            try
            {
                logger?.LogInformation($"Beginning request to {path}");
                return await Handle(path);
            }
            catch (ContractNotFoundError)
            {
                return new NotFoundResult();
            }
            finally
            {
                context.Value = null;
                logger?.LogInformation($"Finished request to {path}");
            }
        }
        public virtual async Task<ActionResult> Handle(string path)
        {
            path = path ?? "";
            while (path.Any() && path.Last() == '/')
                path = path.Substring(0, path.Length - 1);

            var bodyText = new StreamReader(Request.Body).ReadToEnd();
            if (!Channel.TryGetContract(path, out var contract)) return new NotFoundResult();
            contract.Method?.GetCustomAttribute<AuthRequiredAttribute>()?.ThrowIfUnverified();
            var result = await Channel.ReceiveRaw(path, string.IsNullOrEmpty(bodyText) ? null : bodyText);
            if (result.Item1 is ActionResult actionResult)
                return actionResult;
            return new ContentResult() { Content = Channel.Serializer.Serialize(result.Item2.ResponseType, result.Item1), ContentType = "application/json", StatusCode = 200 };
        }
    }
}
