using System.Text.Json;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.Notifications.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.Features.Notifications.Endpoints;

public class NotificationsQueueTrigger(INotificationsDeliveryHandler handler, JsonSerializerOptions jsonSerializerOptions)
{
    [Function(nameof(NotificationsQueueTrigger))]
    public async Task Run([QueueTrigger(StorageConstants.QueueNames.Notifications)] QueueMessage message, CancellationToken cancellationToken)
    {
        var queueItem = JsonSerializer.Deserialize<QueueItem<NotificationEmail>>(message.Body, jsonSerializerOptions);
        await handler.RunAsync(queueItem!, cancellationToken);
    }
}