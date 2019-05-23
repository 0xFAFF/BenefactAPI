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

        public static void ConfigureTypes(IServiceCollection services)
        {
            ReplicationModel.Default.DictionaryAsObject = true;
            services.AddSingleton<IReplicateSerializer<string>>(new JSONGraphSerializer(ReplicationModel.Default));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureTypes(services);
            services.AddCors();
            services.AddMvc();
            services.AddEntityFrameworkNpgsql()
               .AddDbContext<BenefactDbContext>(c => c.UseNpgsql(Configuration.GetConnectionString("BenefactDatabase")));
            services.AddTransient<HTTPChannel>();
            services.AddSingleton<EmailService>();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHandling(services.GetRequiredService<ILogger<Startup>>(), services.GetRequiredService<IReplicateSerializer<string>>());
            app.UseAuthn();
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
                    case "emailtest":
                        var email = services.GetService<EmailService>();
                        email.SendEmail("asherman1024@gmail.com", "Welcome to Benefact!", "verification.html",
                            new Dictionary<string, string> { { "link_target", $"https://{{{{baseURL}}}}/login?nonce=derp" } }).GetAwaiter().GetResult();
                        break;
                    case "mockdata":
                        using(var db = services.GetService<BenefactDbContext>())
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
