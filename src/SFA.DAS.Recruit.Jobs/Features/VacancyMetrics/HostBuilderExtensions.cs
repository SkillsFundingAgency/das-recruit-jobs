using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Recruit.Jobs.Features.VacancyMetrics.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMetrics;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureVacancyMetrics(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<IImportVacancyMetricsHandler, ImportVacancyMetricsHandler>();
        });
    }
}