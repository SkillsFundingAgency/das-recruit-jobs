using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Recruit.Jobs.Features.ProhibitedContentMigration;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureProhibitedContentMigration(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<ReferenceDataMigrationMongoRepository>();
            services.AddTransient<ProhibitedContentMigrationSqlRepository>();
            services.AddTransient<ProhibitedContentMigrationStrategy>();
        });
    }
}