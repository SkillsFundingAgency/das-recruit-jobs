using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Recruit.Jobs.Features.VacancyPublishing.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyPublishing;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureVacancyPublishingFeature(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<IPublishVacancyHandler, PublishVacancyHandler>();
        });
    }
}