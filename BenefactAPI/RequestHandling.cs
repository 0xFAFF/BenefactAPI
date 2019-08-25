using BenefactAPI.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Replicate;
using Replicate.Serialization;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI
{
    [ReplicateType]
    public class ErrorData
    {
        public ErrorData Inner { get; }
        public string Message { get; }
        public string Stack { get; }
        public ErrorData(Exception exception)
        {
            if (exception.InnerException != null)
                Inner = new ErrorData(exception.InnerException);
            Message = exception.Message;
            // TODO: Turn off stack traces in production probably eventually
            Stack = exception.StackTrace;
        }
    }
    public static class RequestHandling
    {
        static (int StatusCode, ErrorData Error) FromException(Exception exception)
            => (exception is HTTPError e ? e.Status : 500, new ErrorData(exception));
        public static void UseHandling(this IApplicationBuilder app, IServiceProvider services, ILogger logger = null)
        {
            var serializer = services.GetRequiredService<IReplicateSerializer>();
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception e)
                {
                    logger?.LogError(e, "Handler exception");
                    // Save the headers from the previous request
                    var headers = context.Response.Headers.ToList();
                    context.Response.Clear();
                    foreach (var kvp in headers)
                        context.Response.Headers.Add(kvp.Key, kvp.Value);
                    var (StatusCode, Error) = FromException(e);
                    context.Response.StatusCode = StatusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(serializer.Serialize(Error).ReadAllString());
                }
            });
        }
    }
}
