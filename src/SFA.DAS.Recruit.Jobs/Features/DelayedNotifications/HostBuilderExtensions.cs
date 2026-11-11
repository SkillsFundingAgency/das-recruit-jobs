using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace SFA.DAS.Recruit.Jobs.Features.DelayedNotifications;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureDelayedNotificationsFeature(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<IRecruitJobsOuterClient, RecruitJobsOuterClient>();
            services.AddTransient<IQueueClient<NotificationEmail>>(serviceProvider =>
            {
                var cfg = serviceProvider.GetService<RecruitJobsConfiguration>()!;
                var queueClient = new QueueClient(cfg.QueueStorage!, StorageConstants.QueueNames.DelayedNotifications);
                var options = serviceProvider.GetService<JsonSerializerOptions>()!;
                return new QueueClient<NotificationEmail>(queueClient, options);
            });
            services.AddTransient<IDelayedNotificationsEnqueueHandler, DelayedNotificationsEnqueueHandler>();
            services.AddTransient<IDelayedNotificationsDeliveryHandler, DelayedNotificationsDeliveryHandler>();
            services.AddTransient<IDeleteNotificationsForInactiveUsersHandler, DeleteNotificationsForInactiveUsersHandler>();

            // register and configure the http client to call apim
            services
                .AddHttpClient<IRecruitJobsOuterClient, RecruitJobsOuterClient>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddStandardResilienceHandler();
        });
    }
}