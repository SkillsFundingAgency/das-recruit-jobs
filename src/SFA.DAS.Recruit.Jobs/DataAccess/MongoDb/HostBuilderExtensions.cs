using System.Diagnostics.CodeAnalysis;
using System.Security.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureMongoDb(this IHostBuilder builder)
    {
        return builder.ConfigureServices((context, services) =>
        {
            var connectionString = context.Configuration.GetConnectionString("MongoDb");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.Configure<MongoDbConnectionDetails>(options =>
                {
                    options.ConnectionString = connectionString;
                });    
            }
            
            MongoDbConventions.RegisterMongoConventions();
            
            services.AddSingleton<IMongoClient>(_ => {
                var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
                settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
                settings.ConnectTimeout = TimeSpan.FromMinutes(10);
                settings.SocketTimeout = TimeSpan.FromMinutes(10);
                return new MongoClient(settings);
            });

            services.AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(MongoDbNames.RecruitDb);
            });
        });
    }
}