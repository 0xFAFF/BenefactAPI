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
        public override IReplicateSerializer<string> Serializer { get; }
            = new JSONGraphSerializer(new ReplicationModel() { DictionaryAsObject = true });

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
        private static AsyncLocal<HttpRequest> currentRequest = new AsyncLocal<HttpRequest>();
        private static AsyncLocal<RouteData> routeData = new AsyncLocal<RouteData>();
        public static T GetRouteParam<T>(string key, Func<string, T> converter)
        {
            if (routeData.Value.Values.TryGetValue(key, out var value))
            {
                try
                {
                    return converter((string)value);
                }
                catch { }
            }
            throw new HTTPError($"Invalid URL param {key}");
        }
        public static T GetQueryParam<T>(string key, Func<string, T> converter)
        {
            if (currentRequest.Value.Query.TryGetValue(key, out var value))
            {
                try
                {
                    return converter(value);
                }
                catch { }
            }
            throw new HTTPError($"Invalid query param {key}");
        }

        public RPCChannel<string, string> Channel;
        IServiceProvider Provider;
        public ReplicateController(IServiceProvider provider)
        {
            Channel = provider.GetRequiredService<HTTPChannel>();
            Channel.RegisterSingleton(new CardsInterface(provider));
            Channel.RegisterSingleton(new CommentsInterface(provider));
            Channel.RegisterSingleton(new ColumnsInterface(provider));
            Channel.RegisterSingleton(new TagsInterface(provider));
            Channel.RegisterSingleton(new UserInterface(provider));
            Channel.Respond<None, string>(Version);
            Provider = provider;
        }

        public Task<string> Version(None _)
        {
            return Task.FromResult(Environment.GetEnvironmentVariable("GIT_COMMIT"));
        }

        // GET api/values
        [HttpGet("{*path}")]
        [HttpPost("{*path}")]
        [HttpOptions("{*path}")]
        public virtual async Task<ActionResult> Post(string path)
        {
            currentRequest.Value = Request;
            routeData.Value = RouteData;
            try
            {
                return await Handle(path);
            }
            catch (ContractNotFoundError)
            {
                return new NotFoundResult();
            }
            finally
            {
                currentRequest.Value = null;
                routeData.Value = null;
            }
        }
        public virtual async Task<ActionResult> Handle(string path)
        {
            while (path.Any() && path.Last() == '/')
                path = path.Substring(0, path.Length - 1);
            using (var serviceScope = Provider.CreateScope())
            {
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
}
