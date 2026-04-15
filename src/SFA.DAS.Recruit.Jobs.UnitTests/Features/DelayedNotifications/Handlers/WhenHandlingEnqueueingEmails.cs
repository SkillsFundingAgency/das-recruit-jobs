using System.Net;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.DelayedNotifications.Handlers;

public class WhenHandlingEnqueueingEmails
{
    [Test, MoqAutoData]
    public async Task Then_A_Cancelled_Token_Will_Prevent_Processing(
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DelayedNotificationsEnqueueHandler sut)
    {
        // arrange
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // act
        await sut.RunAsync(cts.Token);

        // assert
        jobsOuterClient.Verify(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        jobsOuterClient.Verify(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()), Times.Never);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Token_Can_Be_Cancelled_During_Processing(
        List<NotificationEmail> emails,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DelayedNotificationsEnqueueHandler sut)
    {
        // arrange
        var cts = new CancellationTokenSource();
        
        jobsOuterClient
            .Setup(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), cts.Token))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, emails));
        
        jobsOuterClient
            .Setup(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(new ApiResponse(true, HttpStatusCode.NoContent));

        // act
        cts.CancelAfter(TimeSpan.FromSeconds(1));
        await sut.RunAsync(cts.Token);

        // assert
        jobsOuterClient.Verify(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.AtLeast(5));
        jobsOuterClient.Verify(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()), Times.AtLeast(5));
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>()), Times.AtLeast(5));
    }
    
    [Test, MoqAutoData]
    public async Task Then_When_The_Call_To_Get_Emails_Returns_No_Emails_Nothing_Is_Added_To_The_Queue(
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DelayedNotificationsEnqueueHandler sut)
    {
        // arrange
        var cts = new CancellationTokenSource();
        jobsOuterClient
            .Setup(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), cts.Token))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, []));

        // act
        await sut.RunAsync(cts.Token);

        // assert
        jobsOuterClient.Verify(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        jobsOuterClient.Verify(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()), Times.Never);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task Then_When_The_Call_To_Get_Emails_Returns_Null_Nothing_Is_Added_To_The_Queue(
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DelayedNotificationsEnqueueHandler sut)
    {
        // arrange
        var cts = new CancellationTokenSource();
        jobsOuterClient
            .Setup(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), cts.Token))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, null));

        // act
        await sut.RunAsync(cts.Token);

        // assert
        jobsOuterClient.Verify(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        jobsOuterClient.Verify(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()), Times.Never);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task Then_When_The_Call_To_Get_Emails_Fails_Nothing_Is_Added_To_The_Queue(
        List<NotificationEmail> emails,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DelayedNotificationsEnqueueHandler sut)
    {
        // arrange
        var cts = new CancellationTokenSource();
        jobsOuterClient
            .Setup(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), cts.Token))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(false, HttpStatusCode.BadRequest, emails));

        // act
        await sut.RunAsync(cts.Token);

        // assert
        jobsOuterClient.Verify(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        jobsOuterClient.Verify(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()), Times.Never);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Correct_Email_Details_Are_Used(
        NotificationEmail email,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DelayedNotificationsEnqueueHandler sut)
    {
        // arrange
        var cts = new CancellationTokenSource();
        jobsOuterClient
            .SetupSequence(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), cts.Token))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, [email]))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, []));

        IEnumerable<long>? capturedIds = null;
        jobsOuterClient
            .Setup(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()))
            .Callback<IEnumerable<long>>(x => capturedIds = x)
            .ReturnsAsync(new ApiResponse(true, HttpStatusCode.NoContent));

        NotificationEmail? capturedEmail = null;
        queueClient
            .Setup(x => x.SendMessageAsync(It.IsAny<NotificationEmail>()))
            .Callback<NotificationEmail>(x => capturedEmail = x)
            .Returns(Task.CompletedTask);

        // act
        await sut.RunAsync(cts.Token);

        // assert
        capturedIds.Should().BeEquivalentTo(email.SourceIds);
        capturedEmail.Should().BeEquivalentTo(email);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Emails_Are_Added_To_The_Queue(
        List<NotificationEmail> emails,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DelayedNotificationsEnqueueHandler sut)
    {
        // arrange
        var cts = new CancellationTokenSource();
        jobsOuterClient
            .SetupSequence(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), cts.Token))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, emails))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, []));
        
        jobsOuterClient
            .Setup(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(new ApiResponse(true, HttpStatusCode.NoContent));

        // act
        await sut.RunAsync(cts.Token);

        // assert
        jobsOuterClient.Verify(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        jobsOuterClient.Verify(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()), Times.Exactly(3));
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>()), Times.Exactly(emails.Count));
    }
    
    [Test, MoqAutoData]
    public async Task Then_A_Failure_To_Delete_Emails_Stops_Processing_And_Nothing_Is_Added_To_The_Queue(
        List<NotificationEmail> emails,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DelayedNotificationsEnqueueHandler sut)
    {
        // arrange
        var cts = new CancellationTokenSource();
        jobsOuterClient
            .SetupSequence(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), cts.Token))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, emails))
            .ReturnsAsync(new ApiResponse<List<NotificationEmail>>(true, HttpStatusCode.OK, []));
        
        jobsOuterClient
            .Setup(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(new ApiResponse(false, HttpStatusCode.NotFound));

        // act
        await sut.RunAsync(cts.Token);

        // assert
        jobsOuterClient.Verify(x => x.GetDelayedNotificationsBatchBeforeDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        jobsOuterClient.Verify(x => x.DeleteDelayedNotificationsAsync(It.IsAny<IEnumerable<long>>()), Times.Exactly(1));
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>()), Times.Never);
    }
}