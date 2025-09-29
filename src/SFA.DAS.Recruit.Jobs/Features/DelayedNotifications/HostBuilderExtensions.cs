using System.Diagnostics.CodeAnalysis;
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
        return builder.ConfigureServices((context, services) =>
        {
            var sp = services.BuildServiceProvider();
            var serviceConfig = sp.GetService<RecruitJobsConfiguration>();
            
            services.AddTransient<IRecruitJobsOuterClient, RecruitJobsOuterClient>();
            services.AddTransient<IDelayedNotificationQueueClient, DelayedNotificationQueueClient>();
            services.AddTransient<IDelayedNotificationsEnqueueHandler, DelayedNotificationsEnqueueHandler>();
            services.AddTransient<IDelayedNotificationsDeliveryHandler, DelayedNotificationsDeliveryHandler>();

            // register and configure the http client to call apim 
            services
                .AddHttpClient<IRecruitJobsOuterClient, RecruitJobsOuterClient>(httpClient =>
                {
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", serviceConfig!.ApimKey);
                    httpClient.BaseAddress = new Uri(serviceConfig.ApimBaseUrl);
                });
                //.AddPolicyHandler(HttpClientRetryPolicy());
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