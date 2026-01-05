using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyAnalyticsMigration;
[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureVacancyAnalyticsMigration(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<VacancyAnalyticsMigrationMongoRepository>();
            services.AddTransient<VacancyAnalyticsSqlRepository>();
            services.AddTransient<VacancyAnalyticsMigrationStrategy>();
        });
    }
}