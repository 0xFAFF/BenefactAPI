using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenefactAPI.DataAccess;
using BenefactBackend.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BenefactBackend
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
               .AddDbContext<BenefactDBContext>(c => c.UseNpgsql(Configuration.GetConnectionString("BenefactDatabase")))
               .BuildServiceProvider();
            services.AddSingleton<TestImplentation>();
            services.AddSingleton<HTTPChannel>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod());
            }

            app.UseMvc();

            var command = Configuration.GetValue<string>("action");
            if (command != null)
            {
                switch (command)
                {
                    case "install":
                        using (var db = services.GetService<BenefactDBContext>())
                        {
                            db.Database.EnsureDeleted();
                            db.Database.EnsureCreated();
                        }
                        break;
                    case "mockdata":
                        using (var db = services.GetService<BenefactDBContext>())
                        {
                            db.Database.EnsureDeleted();
                            db.Database.EnsureCreated();
                            db.Cards.Add(new CardData()
                            {
                                Id = 1,
                                Title = "Get MD Working",
                                Description = "Some Markdown\n=====\n\n```csharp\n var herp = \"derp\";\n```",
                                ColumnId = 2,
                                TagIds = new[] { 1, 2, 3, 4, 5 }.ToList(),
                            });
                            db.Cards.Add(new CardData()
                            {
                                Id = 2,
                                Title = "Make sure UTF8 works 😑",
                                Description = "😈😈😈😈😈😈",
                                ColumnId = 1,
                                TagIds = new[] { 1 }.ToList(),
                            });
                            db.Cards.Add(new CardData()
                            {
                                Id = 3,
                                Title = "Some Bug",
                                Description = "There was a bug",
                                ColumnId = 2,
                                TagIds = new[] { 4, 2 }.ToList(),
                            });
                            db.Cards.Add(new CardData()
                            {
                                Id = 4,
                                Title = "Fixed Bug",
                                Description = "There was a bug",
                                ColumnId = 3,
                                TagIds = new[] { 4 }.ToList(),
                            });
                            db.Columns.Add(new ColumnData()
                            {
                                Id = 1,
                                Title = "To Do",
                            });
                            db.Columns.Add(new ColumnData()
                            {
                                Id = 2,
                                Title = "In Progress",
                            });
                            db.Columns.Add(new ColumnData()
                            {
                                Id = 3,
                                Title = "Done",
                            });
                            db.Tags.Add(new TagData()
                            {
                                Id = 1,
                                Name = "Story",
                                Color = "#001f3f",
                            });
                            db.Tags.Add(new TagData()
                            {
                                Id = 2,
                                Name = "Dev Task",
                                Color = "#2ECC40",
                            });
                            db.Tags.Add(new TagData()
                            {
                                Id = 3,
                                Name = "Business Boiz",
                                Color = "#FF851B",
                            });
                            db.Tags.Add(new TagData()
                            {
                                Id = 4,
                                Name = "Bug",
                                Character = "bug",
                            });
                            db.Tags.Add(new TagData()
                            {
                                Id = 5,
                                Name = "Star",
                                Color = "#F012BE",
                                Character = "star",
                            });
                            db.SaveChanges();
                        }
                        break;
                }
                Environment.Exit(0);
            }
        }
    }
}
