using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Recruit.Jobs.Features.DeleteStaleVacancies.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;

namespace SFA.DAS.Recruit.Jobs.Features.DeleteStaleVacancies;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureStaleVacanciesToCloseFeature(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<IRecruitJobsOuterClient, RecruitJobsOuterClient>();
            services.AddTransient<IDeleteStaleVacanciesHandler, DeleteStaleVacanciesHandler>();
            // register and configure the http client to call apim
            services
                .AddHttpClient<IRecruitJobsOuterClient, RecruitJobsOuterClient>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddStandardResilienceHandler();
        });
    }
}