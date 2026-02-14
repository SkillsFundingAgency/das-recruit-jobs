using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using SFA.DAS.Recruit.Jobs.Features.VacanciesToClose.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToClose;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureVacanciesToCloseFeature(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<IRecruitJobsOuterClient, RecruitJobsOuterClient>();
            services.AddTransient<ICloseExpiredVacanciesHandler, CloseExpiredVacanciesHandler>();
            // register and configure the http client to call apim
            services
                .AddHttpClient<IRecruitJobsOuterClient, RecruitJobsOuterClient>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(HttpClientRetryPolicy());
        });
    }

    private static AsyncRetryPolicy<HttpResponseMessage> HttpClientRetryPolicy()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 3);
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.BadRequest)
            .WaitAndRetryAsync(delay);
    }
}
