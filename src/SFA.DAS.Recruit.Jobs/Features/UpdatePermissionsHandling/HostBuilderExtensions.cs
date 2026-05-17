using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi.Clients;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureUpdatePermissionsHandlingFeature(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<IQueueClient<TransferVacanciesFromProviderQueueMessage>>(serviceProvider =>
            {
                var cfg = serviceProvider.GetService<RecruitJobsConfiguration>()!;
                var queueClient = new QueueClient(cfg.QueueStorage!, StorageConstants.QueueNames.TransferVacanciesFromProviderQueueName);
                var options = serviceProvider.GetService<JsonSerializerOptions>()!;
                return new QueueClient<TransferVacanciesFromProviderQueueMessage>(queueClient, options);
            });

            services.AddTransient<IQueueClient<TransferVacancyToLegalEntityQueueMessage>>(serviceProvider =>
            {
                var cfg = serviceProvider.GetService<RecruitJobsConfiguration>()!;
                var queueClient = new QueueClient(cfg.QueueStorage!, StorageConstants.QueueNames.TransferVacancyToLegalEntityQueueName);
                var options = serviceProvider.GetService<JsonSerializerOptions>()!;
                return new QueueClient<TransferVacancyToLegalEntityQueueMessage>(queueClient, options);
            });
            
            services.AddTransient<IQueueClient<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage>>(serviceProvider =>
            {
                var cfg = serviceProvider.GetService<RecruitJobsConfiguration>()!;
                var queueClient = new QueueClient(cfg.QueueStorage!, StorageConstants.QueueNames.TransferVacanciesToQaReviewQueueName);
                queueClient.CreateIfNotExists();
                var options = serviceProvider.GetService<JsonSerializerOptions>()!;
                return new QueueClient<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage>(queueClient, options);
            });
            
            services.AddTransient<IQueueClient<TransferVacancyFromEmployerReviewToQaReviewQueueMessage>>(serviceProvider =>
            {
                var cfg = serviceProvider.GetService<RecruitJobsConfiguration>()!;
                var queueClient = new QueueClient(cfg.QueueStorage!, StorageConstants.QueueNames.TransferVacancyToQaReviewQueueName);
                queueClient.CreateIfNotExists();
                var options = serviceProvider.GetService<JsonSerializerOptions>()!;
                return new QueueClient<TransferVacancyFromEmployerReviewToQaReviewQueueMessage>(queueClient, options);
            });
            
            services.AddTransient<IUpdatedPermissionsClient, UpdatedPermissionsClient>();
            services.AddTransient<ITransferVacanciesFromProviderHandler, TransferVacanciesFromProviderHandler>();
            services.AddTransient<ITransferVacancyToLegalEntityHandler, TransferVacancyToLegalEntityHandler>();
            services.AddTransient<ITransferVacanciesToQaReviewHandler, TransferVacanciesToQaReviewHandler>();
            services.AddTransient<ITransferVacancyToQaReviewHandler, TransferVacancyToQaReviewHandler>();
        });
    }
}