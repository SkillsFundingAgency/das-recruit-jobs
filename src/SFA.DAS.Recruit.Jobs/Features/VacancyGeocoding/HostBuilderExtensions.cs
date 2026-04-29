using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Recruit.Jobs.Features.VacancyGeocoding.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyGeocoding;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureVacancyGeocodingFeature(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<IGeocodeService, GeocodeService>();
            services.AddTransient<IGeocodeVacancyHandler, GeocodeVacancyHandler>();
        });
    }
}