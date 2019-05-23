using BenefactAPI;
using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Replicate.MetaData;
using Replicate.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenefactAPITests
{
    public class MockServiceProvider : IServiceProvider
    {
        class Env : IHostingEnvironment
        {
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; }
            public string WebRootPath { get; set; }
            public IFileProvider WebRootFileProvider { get; set; }
            public string ContentRootPath { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; }
        }

        public class Configuration : IConfiguration
        {
            public Dictionary<string, string> Values = new Dictionary<string, string>();
            public string this[string key] { get => Values[key]; set => Values[key] = value; }

            public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();

            public IChangeToken GetReloadToken()
            {
                throw new NotImplementedException();
            }

            public IConfigurationSection GetSection(string key)
            {
                return null;
            }
        }
        class ScopeFactory : IServiceScopeFactory, IServiceScope
        {
            public ScopeFactory(IServiceProvider provider) => ServiceProvider = provider;
            public IServiceProvider ServiceProvider { get; set; }
            public IServiceScope CreateScope() => this;
            public void Dispose() { }
        }

        public JSONGraphSerializer Serializer = new JSONGraphSerializer(new ReplicationModel() { DictionaryAsObject = true });
        DbContextOptions dbOptions = new DbContextOptionsBuilder<BenefactDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        public ConfigurationBuilder Config = new ConfigurationBuilder();
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IReplicateSerializer<string>))
                return Serializer;
            if (serviceType == typeof(BenefactDbContext))
                return new BenefactDbContext(dbOptions, new Env());
            if (serviceType == typeof(IConfiguration))
                return Config.Build();
            if (serviceType == typeof(HTTPChannel))
                return new HTTPChannel(Serializer);
            if (serviceType == typeof(IServiceScopeFactory))
                return new ScopeFactory(this);
            return null;
        }
    }
}
