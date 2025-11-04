using AutoFixture.NUnit3;
using Moq;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.DelayedNotifications.Handlers;

public class WhenHandlingQueuedEmailDelivery
{
    [Test, MoqAutoData]
    public async Task Then_The_Email_Is_Sent(
        NotificationEmail email,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DelayedNotificationsDeliveryHandler sut)
    {
        // arrange
        var queueItem = new QueueItem<NotificationEmail> { Payload = email };

        // act
        await sut.RunAsync(queueItem, CancellationToken.None);

        // assert
        jobsOuterClient.Verify(x => x.SendEmailAsync(ItIs.EquivalentTo(email), It.IsAny<CancellationToken>()), Times.Once);
    }
}