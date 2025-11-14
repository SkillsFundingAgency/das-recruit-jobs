using AutoFixture.NUnit3;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using System.Net;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.DelayedNotifications.Handlers;
[TestFixture]
internal class WhenHandlingDeleteNotificationsForInactiveUsers
{
    [Test, MoqAutoData]
    public async Task RunAsync_Should_DeleteNotifications_ForInactiveUsers_WhenResponseIsValid(
        List<NotificationEmail> emails,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DeleteNotificationsForInactiveUsersHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetDelayedNotificationsBatchByUsersInactiveStatus(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, emails, null));

        jobsOuterClient
            .Setup(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(new ApiResponse(true, HttpStatusCode.NoContent));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()), Times.Exactly(emails.Count));
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseIsNull(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DeleteNotificationsForInactiveUsersHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetDelayedNotificationsBatchByUsersInactiveStatus(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, []));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseFails(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DeleteNotificationsForInactiveUsersHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetDelayedNotificationsBatchByUsersInactiveStatus(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(false, HttpStatusCode.InternalServerError, [], "Internal error"));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_NoNotificationsFound(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DeleteNotificationsForInactiveUsersHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetDelayedNotificationsBatchByUsersInactiveStatus(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, [], null));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()), Times.Never);
    }
}