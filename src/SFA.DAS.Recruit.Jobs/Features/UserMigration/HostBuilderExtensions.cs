using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Recruit.Jobs.Features.UserMigration;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureUserMigration(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<UserMapper>();
            services.AddTransient<UserMigrationMongoRepository>();
            services.AddTransient<UserMigrationSqlRepository>();
            services.AddTransient<UserMigrationStrategy>();
        });
    }
}