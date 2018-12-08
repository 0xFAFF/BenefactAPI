using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenefactAPI.DataAccess;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BenefactBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length == 1 && args[0] == "install")
            {
                using (var db = new BenefactDBContext())
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                }
                Environment.Exit(0);
            }
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
