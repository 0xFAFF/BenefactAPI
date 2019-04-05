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
            services.AddTransient<HTTPChannel>();
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

        void AddCors(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Headers", "*");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<Startup>>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
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
                        var result = FromException(e);
                        context.Response.Clear();
                        AddCors(context.Response);
                        context.Response.StatusCode = result.StatusCode ?? 500;
                        await context.Response.WriteAsync(result.Content);
                    }
                }
            });
            app.UseAuth();
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
                            db.Boards.Add(new BoardData());
                            db.Tags.Add(new TagData()
                            {
                                Name = "Story",
                                Color = "#001f3f",
                                BoardId = 1,
                            });
                            db.Tags.Add(new TagData()
                            {
                                Name = "Dev Task",
                                Color = "#2ECC40",
                                BoardId = 1,
                            });
                            db.Tags.Add(new TagData()
                            {
                                Name = "Business Boiz",
                                Color = "#FF851B",
                                BoardId = 1,
                            });
                            db.Tags.Add(new TagData()
                            {
                                Name = "Bug",
                                Character = "bug",
                                BoardId = 1,
                            });
                            db.Tags.Add(new TagData()
                            {
                                Name = "Star",
                                Color = "#F012BE",
                                Character = "star",
                                BoardId = 1,
                            });
                            db.SaveChanges();
                        }
                        var cards = new CardsInterface(services);
                        var users = new UserInterface(services);
                        var columns = new ColumnsInterface(services);
                        var todo = columns.Add(new ColumnData()
                        {
                            Title = "To Do",
                            BoardId = 1,
                        }).GetAwaiter().GetResult();
                        var inp = columns.Add(new ColumnData()
                        {
                            Title = "In Progress",
                            BoardId = 1,
                        }).GetAwaiter().GetResult();
                        var done = columns.Add(new ColumnData()
                        {
                            Title = "Done",
                            BoardId = 1,
                        }).GetAwaiter().GetResult();
                        cards.Add(new CardData()
                        {
                            Title = "Get MD Working",
                            Description = "Some Markdown\n=====\n\n```csharp\n var herp = \"derp\";\n```",
                            ColumnId = 2,
                            TagIds = new[] { 1, 2, 3, 4, 5 }.ToList(),
                            BoardId = 1,
                        }).GetAwaiter().GetResult();
                        cards.Add(new CardData()
                        {
                            Title = "Make sure UTF8 works 😑",
                            Description = "😈😈😈😈😈😈",
                            ColumnId = 1,
                            TagIds = new[] { 1 }.ToList(),
                            BoardId = 1,
                        }).GetAwaiter().GetResult();
                        cards.Add(new CardData()
                        {
                            Title = "Some Bug",
                            Description = "There was a bug",
                            ColumnId = 2,
                            TagIds = new[] { 4, 2 }.ToList(),
                            BoardId = 1,
                        }).GetAwaiter().GetResult();
                        cards.Add(new CardData()
                        {
                            Title = "Fixed Bug",
                            Description = "There was a bug",
                            ColumnId = 3,
                            TagIds = new[] { 4 }.ToList(),
                            BoardId = 1,
                        }).GetAwaiter().GetResult();
                        users.Add(new UserCreateRequest()
                        {
                            Email = "faff@faff.faff",
                            Name = "FAFF",
                            Password = "fafffaff",
                        }, false).GetAwaiter().GetResult();
                        services.DoWithDB(async db =>
                        {
                            var faff = await db.Users.FirstOrDefaultAsync(u => u.Name == "FAFF");
                            faff.EmailVerified = true;
                            return true;
                        }).GetAwaiter().GetResult();
                        break;
                }
                Environment.Exit(0);
            }
        }
    }
}
