using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenefactAPI.DataAccess;
using BenefactAPI.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Replicate.MetaData;
using Replicate.Serialization;
using Replicate.RPC;
using Replicate;
using BenefactAPI.RPCInterfaces;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;
using Replicate.Web;

namespace BenefactAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ReplicationModel.Default.DictionaryAsObject = true;
            ReplicationModel.Default.LoadTypes(typeof(BoardData).Assembly);
            var serializer = new JSONSerializer(ReplicationModel.Default);
            services.AddSingleton<IReplicateSerializer>(new JSONSerializer(ReplicationModel.Default));
            services.AddSingleton(serializer);
            services.AddEntityFrameworkNpgsql()
               .AddDbContext<BenefactDbContext>(c => c.UseNpgsql(Configuration.GetConnectionString("BenefactDatabase")));
            services.AddSingleton<EmailService>();
            services.AddReplicate(serializer);
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
                context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                if (context.Request.Method != "OPTIONS")
                    await next();
            });
            app.UseEndpointRouting();
            app.UseErrorHandling(services.GetRequiredService<IReplicateSerializer>());
            app.UseAuthn();
            app.UseBoards();
            app.UseEndpoint();

            var command = Configuration.GetValue<string>("action");
            if (command != null)
            {
                switch (command)
                {
                    case "destroy":
                        using (var db = services.GetService<BenefactDbContext>())
                            db.Database.EnsureDeleted();
                        break;
                    case "emailtest":
                        var email = services.GetService<EmailService>();
                        email.SendEmail("asherman1024@gmail.com", "Welcome to Benefact!", "verification.html",
                            new Dictionary<string, string> { { "link_target", $"https://{{{{baseURL}}}}/login?nonce=derp" } }).GetAwaiter().GetResult();
                        break;
                    case "migrate":
                        using (var db = services.GetService<BenefactDbContext>())
                            db.Database.Migrate();
                        break;
                    case "mockdata":
                        using (var db = services.GetService<BenefactDbContext>())
                        {
                            db.Database.EnsureDeleted();
                            db.Database.Migrate();
                        }
                        MockData.AddToDb(services);
                        break;
                }
                Environment.Exit(0);
            }
        }
    }
}
