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
               .AddDbContext<BenefactDBContext>(c => c.UseNpgsql(Configuration.GetConnectionString("BenefactDatabase")))
               .BuildServiceProvider();
            services.AddSingleton<CardsInterface>();
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
                            var todo = db.Columns.Add(new ColumnData()
                            {
                                Title = "To Do",
                            }).Entity;
                            var inp = db.Columns.Add(new ColumnData()
                            {
                                Title = "In Progress",
                            }).Entity;
                            var done = db.Columns.Add(new ColumnData()
                            {
                                Title = "Done",
                            }).Entity;
                            db.Cards.Add(new CardData()
                            {
                                Title = "Get MD Working",
                                Description = "Some Markdown\n=====\n\n```csharp\n var herp = \"derp\";\n```",
                                Column = inp,
                                TagIds = new[] { 1, 2, 3, 4, 5 }.ToList(),
                            });
                            db.Cards.Add(new CardData()
                            {
                                Title = "Make sure UTF8 works 😑",
                                Description = "😈😈😈😈😈😈",
                                Column = todo,
                                TagIds = new[] { 1 }.ToList(),
                            });
                            db.Cards.Add(new CardData()
                            {
                                Title = "Some Bug",
                                Description = "There was a bug",
                                Column = inp,
                                TagIds = new[] { 4, 2 }.ToList(),
                            });
                            db.Cards.Add(new CardData()
                            {
                                Title = "Fixed Bug",
                                Description = "There was a bug",
                                Column = done,
                                TagIds = new[] { 4 }.ToList(),
                            });
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
                        break;
                }
                Environment.Exit(0);
            }
        }
    }
}
