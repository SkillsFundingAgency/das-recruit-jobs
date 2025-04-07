using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Recruit.Jobs.Features.EmployerProfilesMigration;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureEmployerProfilesMigration(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<EmployerProfilesMigrationMongoRepository>();
            services.AddTransient<EmployerProfilesMigrationSqlRepository>();
            services.AddTransient<EmployerProfilesMapper>();
            services.AddTransient<EmployerProfilesMigrationStrategy>();
        });
    }
}