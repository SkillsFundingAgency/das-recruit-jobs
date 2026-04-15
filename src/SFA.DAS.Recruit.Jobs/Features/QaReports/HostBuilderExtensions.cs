using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Recruit.Jobs.Features.QaReports;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureQaReportsMigration(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<ReportMapper>();
            services.AddTransient<QaReportsMigrationMongoRepository>();
            services.AddTransient<QaReportsMigrationSqlRepository>();
            services.AddTransient<QaReportsMigrationStrategy>();
        });
    }
}
