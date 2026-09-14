using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Recruit.Jobs.Features.Reports.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.Reports;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureGenerateReportFeature(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<IReportCreatedHandler, ReportCreatedHandler>();
        });
    }
}