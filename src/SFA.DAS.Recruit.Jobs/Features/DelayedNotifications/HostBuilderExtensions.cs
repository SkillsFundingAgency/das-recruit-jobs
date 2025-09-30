using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Clients;
using SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;

namespace SFA.DAS.Recruit.Jobs.Features.DelayedNotifications;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureDelayedNotificationsFeature(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<IRecruitJobsOuterClient, RecruitJobsOuterClient>();
            services.AddTransient<IDelayedNotificationQueueClient>(serviceProvider =>
            {
                var cfg = serviceProvider.GetService<RecruitJobsConfiguration>()!;
                var queueClient = new QueueClient(cfg.QueueStorage, StorageConstants.QueueNames.DelayedNotifications);
                var options = serviceProvider.GetService<JsonSerializerOptions>()!;
                return new DelayedNotificationQueueClient(queueClient, options);
            });
            services.AddTransient<IDelayedNotificationsEnqueueHandler, DelayedNotificationsEnqueueHandler>();
            services.AddTransient<IDelayedNotificationsDeliveryHandler, DelayedNotificationsDeliveryHandler>();

            // register and configure the http client to call apim
            services
                .AddHttpClient<IRecruitJobsOuterClient, RecruitJobsOuterClient>((serviceProvider, httpClient) =>
                {
                    var cfg = serviceProvider.GetService<RecruitJobsConfiguration>()!;
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", cfg.ApimKey);
                    httpClient.BaseAddress = new Uri(cfg.ApimBaseUrl);
                })
                .AddPolicyHandler(HttpClientRetryPolicy());
        });
    }
    
    private static IAsyncPolicy<HttpResponseMessage> HttpClientRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.BadRequest)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}