using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Endpoints;

[ExcludeFromCodeCoverage]
public class DelayedNotificationsQueueTrigger(
    ILogger<DelayedNotificationsQueueTrigger> logger,
    IDelayedNotificationsDeliveryHandler handler,
    JsonSerializerOptions jsonSerializerOptions)
{
    private const string TriggerName = nameof(DelayedNotificationsQueueTrigger);
    
    [Function(TriggerName)]
    public async Task Run([QueueTrigger(StorageConstants.QueueNames.DelayedNotifications)] QueueMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            var queueItem = JsonSerializer.Deserialize<QueueItem<NotificationEmail>>(message.Body, jsonSerializerOptions);
            await handler.RunAsync(queueItem!, cancellationToken);
        }
        catch (JsonException)
        {
            logger.LogError("[{TriggerName}] Error deserializing delayed notification email queue item message id: {MessageId}", TriggerName, message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured whilst sending email", TriggerName);
        }
        finally
        {
            logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
        }
    }
}