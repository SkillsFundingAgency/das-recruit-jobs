using System.Text.Json;
using Azure.Storage.Queues;

namespace SFA.DAS.Recruit.Jobs.Core.Infrastructure;

public interface IQueueClient<in T>
{
    Task SendMessageAsync(T item);
}

public class QueueClient<T> : IQueueClient<T>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly QueueClient _queueClient;

    internal QueueClient(QueueClient queueClient, JsonSerializerOptions jsonSerializerOptions)
    {
        _queueClient = queueClient;
        _jsonSerializerOptions = jsonSerializerOptions;
        _queueClient.CreateIfNotExists();
    }
    
    public async Task SendMessageAsync(T item)
    {
        var queueItem = new QueueItem<T> { Payload = item, };
        var message = JsonSerializer.Serialize(queueItem, _jsonSerializerOptions);
        await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message)));
    }
}