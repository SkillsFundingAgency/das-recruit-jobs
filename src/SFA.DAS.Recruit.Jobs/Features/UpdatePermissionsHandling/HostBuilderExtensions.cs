using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Domain;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureUpdatePermissionsHandlingFeature(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<ITransferVacanciesFromProviderHandler, TransferVacanciesFromProviderHandler>();
            
            services.AddTransient<IQueueClient<TransferVacanciesFromProviderQueueMessage>>(serviceProvider =>
            {
                var cfg = serviceProvider.GetService<RecruitJobsConfiguration>()!;
                var queueClient = new QueueClient(cfg.QueueStorage!, StorageConstants.QueueNames.TransferVacanciesFromProviderQueueName);
                var options = serviceProvider.GetService<JsonSerializerOptions>()!;
                return new QueueClient<TransferVacanciesFromProviderQueueMessage>(queueClient, options);
            });
        });
    }
}