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
        // act
        await sut.RunAsync(email, CancellationToken.None);

        // assert
        jobsOuterClient.Verify(x => x.SendEmailAsync(ItIs.EquivalentTo(email), It.IsAny<CancellationToken>()), Times.Once);
    }
}