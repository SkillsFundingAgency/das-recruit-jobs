using System.Diagnostics.CodeAnalysis;
using System.Security.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureApplicationReviewsMigration(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<ApplicationReviewsMigrationSqlRepository>();
            services.AddTransient<ApplicationReviewMapper>();
            services.AddTransient<LegacyApplicationMapper>();
            services.AddTransient<ApplicationReviewsMigrationMongoRepository>();
            services.AddTransient<ApplicationReviewMigrationStrategy>();
            
            services.AddSingleton<IMongoClient>(sp => {
                var connectionString = _.Configuration.GetConnectionString("MongoDb");
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