using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureAiVacancyReviewingFeature(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services
                .AddHttpClient<IRecruitAiOuterClient, RecruitAiOuterClient>()
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