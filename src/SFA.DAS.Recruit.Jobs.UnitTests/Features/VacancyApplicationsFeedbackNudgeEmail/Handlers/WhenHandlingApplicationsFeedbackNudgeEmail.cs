using System.Net;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.VacancyApplicationsFeedbackNudgeEmail.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.VacancyApplicationsFeedbackNudgeEmail.Handlers;

public class WhenHandlingApplicationsFeedbackNudgeEmail
{
    [Test, MoqAutoData]
    public async Task Then_If_No_Vacancies_Need_The_Nudge_Email_Then_Nothing_Is_Processed(
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] ApplicationsFeedbackNudgeEmailHandler sut)
    {
        // arrange
        jobsOuterClient
            .Setup(x => x.GetAsync<List<VacancyApplicationsCountRequiringFeedback>>(It.IsAny<GetVacanciesWithApplicationsNeedingFeedbackForDate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<VacancyApplicationsCountRequiringFeedback>>(HttpStatusCode.OK, []));

        // act
        await sut.RunAsync(CancellationToken.None);

        // assert
        jobsOuterClient.Verify(x => x.PostAsync<DataResponse<List<NotificationEmail>>>(It.IsAny<PostCreateVacancyFeedbackNudgeNotifications>(), CancellationToken.None), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_No_Notifications_Are_Returned_Then_No_Messages_Are_Sent(
        List<VacancyApplicationsCountRequiringFeedback> vacancies,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Greedy] ApplicationsFeedbackNudgeEmailHandler sut)
    {
        // arrange
        jobsOuterClient
            .Setup(x => x.GetAsync<List<VacancyApplicationsCountRequiringFeedback>>(It.IsAny<GetVacanciesWithApplicationsNeedingFeedbackForDate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<VacancyApplicationsCountRequiringFeedback>>(HttpStatusCode.OK, vacancies));
        
        jobsOuterClient
            .Setup(x => x.PostAsync<DataResponse<List<NotificationEmail>>>(It.IsAny<PostCreateVacancyFeedbackNudgeNotifications>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<DataResponse<List<NotificationEmail>>>(HttpStatusCode.OK, new DataResponse<List<NotificationEmail>>([])));

        // act
        await sut.RunAsync(CancellationToken.None);

        // assert
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Messages_Are_Sent(
        List<VacancyApplicationsCountRequiringFeedback> vacancies,
        List<NotificationEmail> notificationEmails,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Greedy] ApplicationsFeedbackNudgeEmailHandler sut)
    {
        // arrange
        jobsOuterClient
            .Setup(x => x.GetAsync<List<VacancyApplicationsCountRequiringFeedback>>(It.IsAny<GetVacanciesWithApplicationsNeedingFeedbackForDate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<VacancyApplicationsCountRequiringFeedback>>(HttpStatusCode.OK, vacancies));
        
        jobsOuterClient
            .Setup(x => x.PostAsync<DataResponse<List<NotificationEmail>>>(It.IsAny<PostCreateVacancyFeedbackNudgeNotifications>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<DataResponse<List<NotificationEmail>>>(HttpStatusCode.OK, new DataResponse<List<NotificationEmail>>(notificationEmails)));
        
        var capturedMessages = new List<NotificationEmail>();
        queueClient
            .Setup(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationEmail, CancellationToken>((message, _) => capturedMessages.Add(message));

        // act
        await sut.RunAsync(CancellationToken.None);

        // assert
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), It.IsAny<CancellationToken>()), Times.Exactly(notificationEmails.Count));
        capturedMessages.Should().BeEquivalentTo(notificationEmails);
    }
}