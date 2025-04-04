using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureUserNotificationPreferencesMigration(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<UserNotificationPreferencesMigrationMongoRepository>();
            services.AddTransient<UserNotificationPreferencesMigrationSqlRepository>();
            services.AddTransient<UserNotificationPreferencesMigrationStrategy>();
        });
    }
}