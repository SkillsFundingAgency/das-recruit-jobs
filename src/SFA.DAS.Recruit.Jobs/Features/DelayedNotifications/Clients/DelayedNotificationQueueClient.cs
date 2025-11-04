using System.Text.Json;
using Azure.Storage.Queues;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Clients;

public interface IDelayedNotificationQueueClient
{
    Task SendMessageAsync(NotificationEmail item);
}

public class DelayedNotificationQueueClient : IDelayedNotificationQueueClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly QueueClient _queueClient;

    public DelayedNotificationQueueClient(QueueClient queueClient, JsonSerializerOptions jsonSerializerOptions)
    {
        _queueClient = queueClient;
        _jsonSerializerOptions = jsonSerializerOptions;
        _queueClient.CreateIfNotExists();
    }
    
    public async Task SendMessageAsync(NotificationEmail item)
    {
        var queueItem = new QueueItem<NotificationEmail> { Payload = item, };
        var message = JsonSerializer.Serialize(queueItem, _jsonSerializerOptions);
        await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message)));
    }
}