using System.Text.Json;
using Azure.Storage.Queues;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Clients;

public interface IDelayedNotificationQueueClient
{
    Task SendMessageAsync(NotificationEmail items);
}

public class DelayedNotificationQueueClient : IDelayedNotificationQueueClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly QueueClient _queueClient;

    public DelayedNotificationQueueClient(RecruitJobsConfiguration config, JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
        _queueClient = new QueueClient(config.QueueStorage, StorageConstants.QueueNames.DelayedNotifications);
        _queueClient.CreateIfNotExists();
    }
    
    public async Task SendMessageAsync(NotificationEmail item)
    {
        var queueItem = new QueueItem<NotificationEmail> { Payload = item, };
        var message = JsonSerializer.Serialize(queueItem, _jsonSerializerOptions);
        await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message)));
    }
}