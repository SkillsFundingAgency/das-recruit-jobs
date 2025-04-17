using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyReviewMigration;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureVacancyReviewMigration(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<VacancyReviewMapper>();
            services.AddTransient<VacancyReviewMigrationMongoRepository>();
            services.AddTransient<VacancyReviewMigrationSqlRepository>();
            services.AddTransient<VacancyReviewMigrationStrategy>();
        });
    }
}