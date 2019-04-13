using BenefactAPI.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Replicate;
using Replicate.Serialization;
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

        static void AddCors(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Headers", "*");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
        }
        public static void UseHandling(this IApplicationBuilder app, ILogger logger, IReplicateSerializer<string> serializer)
        {
            app.Use(async (context, next) =>
            {
                AddCors(context.Response);
                if (context.Request.Method != "OPTIONS")
                {
                    try
                    {
                        await next.Invoke();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Handler exception");
                        context.Response.Clear();
                        AddCors(context.Response);
                        var (StatusCode, Error) = FromException(e);
                        context.Response.StatusCode = StatusCode;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(serializer.Serialize(Error));
                    }
                }
            });
        }
    }
}
