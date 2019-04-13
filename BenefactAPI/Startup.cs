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
            services.AddSingleton<IReplicateSerializer<string>>(new JSONGraphSerializer(ReplicationModel.Default));
            services.AddCors();
            services.AddMvc();
            services.AddEntityFrameworkNpgsql()
               .AddDbContext<BenefactDbContext>(c => c.UseNpgsql(Configuration.GetConnectionString("BenefactDatabase")))
               .BuildServiceProvider();
            services.AddTransient<HTTPChannel>();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHandling(services.GetRequiredService<ILogger<Startup>>(), services.GetRequiredService<IReplicateSerializer<string>>());
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
                        var users = new UserInterface(services);
                        using (var db = services.GetService<BenefactDbContext>())
                        {
                            db.Database.EnsureDeleted();
                            db.Database.Migrate();
                            (db.Boards.FirstOrDefault() ?? db.Boards.Add(new BoardData()).Entity).DefaultPrivileges = (Privileges)255;
                            var board2 = db.Boards.Skip(1).FirstOrDefault() ?? db.Boards.Add(new BoardData()).Entity;
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
                            db.Columns.Add(new ColumnData()
                            {
                                Title = "To Do",
                                Index = 1,
                                BoardId = 1,
                            });
                            db.Columns.Add(new ColumnData()
                            {
                                Title = "In Progress",
                                Index = 2,
                                BoardId = 1,
                            });
                            db.Columns.Add(new ColumnData()
                            {
                                Title = "Done",
                                Index = 3,
                                BoardId = 1,
                            });
                            db.SaveChanges();
                            db.Cards.Add(new CardData()
                            {
                                Title = "Get MD Working",
                                Description = "Some Markdown\n=====\n\n```csharp\n var herp = \"derp\";\n```",
                                ColumnId = 2,
                                TagIds = new[] { 1, 2, 3, 4, 5 }.ToList(),
                                BoardId = 1,
                                Index = 1,
                            });
                            db.Cards.Add(new CardData()
                            {
                                Title = "Make sure UTF8 works 😑",
                                Description = "😈😈😈😈😈😈",
                                ColumnId = 1,
                                TagIds = new[] { 1 }.ToList(),
                                BoardId = 1,
                                Index = 2,
                            });
                            db.Cards.Add(new CardData()
                            {
                                Title = "Some Bug",
                                Description = "There was a bug",
                                ColumnId = 2,
                                TagIds = new[] { 4, 2 }.ToList(),
                                BoardId = 1,
                                Index = 3,
                            });
                            db.Cards.Add(new CardData()
                            {
                                Title = "Fixed Bug",
                                Description = "There was a bug",
                                ColumnId = 3,
                                TagIds = new[] { 4 }.ToList(),
                                BoardId = 1,
                                Index = 4,
                            });
                            db.SaveChanges();
                        }
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
