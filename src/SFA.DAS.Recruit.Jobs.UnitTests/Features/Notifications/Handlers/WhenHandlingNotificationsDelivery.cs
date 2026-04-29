using AutoFixture.NUnit3;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.Notifications.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.Notifications.Handlers;

public class WhenHandlingNotificationsDelivery
{
    [Test, MoqAutoData]
    public async Task Then_The_Message_Is_Handled(
        QueueItem<NotificationEmail> message,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] NotificationsDeliveryHandler sut)
    {
        // act
        await sut.RunAsync(message, CancellationToken.None);

        // assert
        jobsOuterClient.Verify(x => x.PostAsync(It.Is<PostSendNotificationRequest>(r => r.Data == message.Payload), It.IsAny<CancellationToken>()), Times.Once);
    }
}