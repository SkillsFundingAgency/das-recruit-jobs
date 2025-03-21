using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureApplicationReviewsMigration(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<ApplicationReviewMapper>();
            services.AddTransient<LegacyApplicationMapper>();
            services.AddTransient<ApplicationReviewsMigrationMongoRepository>();
            services.AddTransient<ApplicationReviewMigrationStrategy>();
        });
    }
}