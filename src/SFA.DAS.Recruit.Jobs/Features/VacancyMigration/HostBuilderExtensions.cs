using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureVacancyMigration(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddSingleton<UserLocator>(); // singleton so the cache persists - let's see if this causes memory problems on the job
            services.AddTransient<VacancyMapper>();
            services.AddTransient<VacancyMigrationMongoRepository>();
            services.AddTransient<VacancyMigrationSqlRepository>();
            services.AddTransient<VacancyMigrationStrategy>();
        });
    }
}