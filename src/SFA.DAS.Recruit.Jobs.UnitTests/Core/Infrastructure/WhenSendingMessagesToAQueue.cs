using System.Text.Json;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Core.Infrastructure;

public class ResponseMock<T> : Response<T>
{
    public override Response GetRawResponse()
    {
        // we don't care about this for testing
        throw new NotImplementedException();
    }
}

public class WhenSendingMessagesToAQueue
{
    [Test, MoqAutoData]
    public async Task Then_The_Message_Is_Sent_To_The_Queue()
    {
        // arrange
        var queueClient = new Mock<QueueClient>();
        var sut = new QueueClient<string>(queueClient.Object, new JsonSerializerOptions());
        
        string? capturedMessage = null;
        queueClient
            .Setup(x => x.SendMessageAsync(It.IsAny<string>()))
            .Callback((string message) => capturedMessage = message)
            .ReturnsAsync(new ResponseMock<SendReceipt>());

        // act
        await sut.SendMessageAsync("some text");

        // assert
        capturedMessage.Should().NotBeNull();
        var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(capturedMessage));
        var queueItem = JsonSerializer.Deserialize<QueueItem<string>>(json);
        queueItem.Should().NotBeNull();
        queueItem.Payload.Should().Be("some text");
    }
}