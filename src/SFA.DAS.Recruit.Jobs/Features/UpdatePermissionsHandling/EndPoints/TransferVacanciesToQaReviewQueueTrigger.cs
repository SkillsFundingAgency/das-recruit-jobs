using System.Text.Json;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.EndPoints;

public class TransferVacanciesToQaReviewQueueTrigger(ITransferVacanciesToQaReviewHandler handler, JsonSerializerOptions jsonSerializerOptions)
{
    private const string TriggerName = nameof(TransferVacanciesToQaReviewQueueTrigger);
    
    [Function(TriggerName)]
    public async Task Run([QueueTrigger(StorageConstants.QueueNames.TransferVacanciesToQaReviewQueueName)] QueueMessage message, CancellationToken cancellationToken)
    {
        var queueItem = JsonSerializer.Deserialize<QueueItem<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage>>(message.Body, jsonSerializerOptions);
        if (queueItem is null)
        {
            throw new JsonException($"Failed to deserialise TransferVacanciesFromEmployerReviewToQaReviewQueueMessage '{message.MessageId}'");
        }
        await handler.RunAsync(queueItem!.Payload, cancellationToken);
    }
}