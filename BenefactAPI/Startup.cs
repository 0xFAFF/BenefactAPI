using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenefactAPI.DataAccess;
using BenefactAPI.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

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
            services.AddCors();
            services.AddMvc();
            services.AddEntityFrameworkNpgsql()
               .AddDbContext<BenefactDbContext>(c => c.UseNpgsql(Configuration.GetConnectionString("BenefactDatabase")))
               .BuildServiceProvider();
            services.AddSingleton<HTTPChannel>();
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
                {
                    try
                    {
                        await next.Invoke();
                    }
                    catch(Exception e)
                    {
                        var result = FromException(e);
                        context.Response.Clear();
                        context.Response.StatusCode = result.StatusCode ?? 500;
                        await context.Response.WriteAsync(result.Content);
                    }
                }
            });
            app.UseMvc();

            var command = Configuration.GetValue<string>("action");
            if (command != null)
            {
                switch (command)
                {
                    case "destroy":
                        using (var db = services.GetService<BenefactDbContext>())
                            db.Database.EnsureDeleted();
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
                            db.Tags.Add(new TagData()
                            {
                                Name = "Story",
                                Color = "#001f3f",
                            });
                            db.Tags.Add(new TagData()
                            {
                                Name = "Dev Task",
                                Color = "#2ECC40",
                            });
                            db.Tags.Add(new TagData()
                            {
                                Name = "Business Boiz",
                                Color = "#FF851B",
                            });
                            db.Tags.Add(new TagData()
                            {
                                Name = "Bug",
                                Character = "bug",
                            });
                            db.Tags.Add(new TagData()
                            {
                                Name = "Star",
                                Color = "#F012BE",
                                Character = "star",
                            });
                            db.SaveChanges();
                        }
                        var cards = services.GetService<CardsInterface>();
                        var users = services.GetService<UserInterface>();
                        var todo = cards.AddColumn(new ColumnData()
                        {
                            Title = "To Do",
                        }).GetAwaiter().GetResult();
                        var inp = cards.AddColumn(new ColumnData()
                        {
                            Title = "In Progress",
                        }).GetAwaiter().GetResult();
                        var done = cards.AddColumn(new ColumnData()
                        {
                            Title = "Done",
                        }).GetAwaiter().GetResult();
                        cards.AddCard(new CardData()
                        {
                            Title = "Get MD Working",
                            Description = "Some Markdown\n=====\n\n```csharp\n var herp = \"derp\";\n```",
                            ColumnId = 2,
                            TagIds = new[] { 1, 2, 3, 4, 5 }.ToList(),
                        }).GetAwaiter().GetResult();
                        cards.AddCard(new CardData()
                        {
                            Title = "Make sure UTF8 works 😑",
                            Description = "😈😈😈😈😈😈",
                            ColumnId = 1,
                            TagIds = new[] { 1 }.ToList(),
                        }).GetAwaiter().GetResult();
                        cards.AddCard(new CardData()
                        {
                            Title = "Some Bug",
                            Description = "There was a bug",
                            ColumnId = 2,
                            TagIds = new[] { 4, 2 }.ToList(),
                        }).GetAwaiter().GetResult();
                        cards.AddCard(new CardData()
                        {
                            Title = "Fixed Bug",
                            Description = "There was a bug",
                            ColumnId = 3,
                            TagIds = new[] { 4 }.ToList(),
                        }).GetAwaiter().GetResult();
                        users.CreateUser(new UserCreateRequest()
                        {
                            Email = "faff@faff.faff",
                            Name = "FAFF",
                            Password = "fafffaff",
                        }).GetAwaiter().GetResult();
                        break;
                }
                Environment.Exit(0);
            }
        }
    }
}
