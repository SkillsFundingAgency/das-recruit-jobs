using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Recruit.Jobs.Features.BlockedOrganisationsMigration;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureBlockedOrganisationsMigration(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<BlockedOrganisationMigrationMongoRepository>();
            services.AddTransient<BlockedOrganisationMigrationSqlRepository>();
            services.AddTransient<BlockedOrganisationMapper>();
            services.AddTransient<BlockedOrganisationMigrationStrategy>();
        });
    }
}