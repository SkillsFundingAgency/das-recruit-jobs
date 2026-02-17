using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;

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
            
            services.AddTransient<IQueueClient<AiVacancyReviewMessage>>(serviceProvider =>
            {
                var cfg = serviceProvider.GetService<RecruitJobsConfiguration>()!;
                var queueClient = new QueueClient(cfg.QueueStorage!, StorageConstants.QueueNames.AiVacancyReviewRequests);
                var options = serviceProvider.GetService<JsonSerializerOptions>()!;
                return new QueueClient<AiVacancyReviewMessage>(queueClient, options);
            });
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