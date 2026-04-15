using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.EndPoints;

[ExcludeFromCodeCoverage]
public class TransferVacanciesFromProviderQueueTrigger(
    ILogger<TransferVacanciesFromProviderQueueTrigger> logger,
    ITransferVacanciesFromProviderHandler handler,
    JsonSerializerOptions jsonSerializerOptions)
{
    private const string TriggerName = nameof(TransferVacanciesFromProviderQueueTrigger);
    
    [Function(TriggerName)]
    public async Task Run([QueueTrigger(StorageConstants.QueueNames.TransferVacanciesFromProviderQueueName)] QueueMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            var queueItem = JsonSerializer.Deserialize<QueueItem<TransferVacanciesFromProviderQueueMessage>>(message.Body, jsonSerializerOptions);
            if (queueItem is null)
            {
                throw new JsonException($"Failed to deserialise TransferVacanciesFromProviderQueueMessage '{message.MessageId}'");
            }
            await handler.RunAsync(queueItem.Payload, cancellationToken);
        }
        finally
        {
            logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
        }
    }
}