using System.Net;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;
using SFA.DAS.Recruit.Jobs.Services;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Services;

public class WhenCreatingVacancyNotifications
{
    [Test, MoqAutoData]
    public async Task Then_The_Call_Succeeds_And_The_Items_Are_Returned(
        Guid id,
        DataResponse<List<NotificationEmail>> dataResponse,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] NotificationService sut)
    {
        // arrange
        var expectedRequest = new PostCreateVacancyNotificationsRequest(id);
        PostCreateVacancyNotificationsRequest? capturedRequest = null;
        jobsOuterClient
            .Setup(x => x.PostAsync<DataResponse<List<NotificationEmail>>>(It.IsAny<PostCreateVacancyNotificationsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IPostRequest, CancellationToken>((x, _) => capturedRequest = x as PostCreateVacancyNotificationsRequest)
            .ReturnsAsync(new ApiResponse<DataResponse<List<NotificationEmail>>>(HttpStatusCode.OK, dataResponse));

        // act
        var response = await sut.CreateVacancyNotificationsAsync(id, null, CancellationToken.None);

        // assert
        response.Should().HaveCount(dataResponse.Data.Count);
        capturedRequest.Should().BeEquivalentTo(expectedRequest);
    }
    
    [Test, MoqAutoData]
    public async Task Then_An_Exception_Is_Thrown_When_The_Call_Fails(
        Guid id,
        DataResponse<List<NotificationEmail>> dataResponse,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] NotificationService sut)
    {
        // arrange
        jobsOuterClient
            .Setup(x => x.PostAsync<DataResponse<List<NotificationEmail>>>(It.IsAny<PostCreateVacancyNotificationsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<DataResponse<List<NotificationEmail>>>(HttpStatusCode.BadRequest, dataResponse));

        // act
        var action = async () => await sut.CreateVacancyNotificationsAsync(id, null, CancellationToken.None);

        // assert
        await action.Should().ThrowAsync<ApiException>();
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Status_Is_Passed_The_Items_Are_Returned(
        Guid id,
        DataResponse<List<NotificationEmail>> dataResponse,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] NotificationService sut)
    {
        // arrange
        var expectedRequest = new PostCreateVacancyNotificationsByStatusRequest(id, VacancyStatus.Approved);
        PostCreateVacancyNotificationsByStatusRequest? capturedRequest = null;
        jobsOuterClient
            .Setup(x => x.PostAsync<DataResponse<List<NotificationEmail>>>(It.IsAny<PostCreateVacancyNotificationsByStatusRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IPostRequest, CancellationToken>((x, _) => capturedRequest = x as PostCreateVacancyNotificationsByStatusRequest)
            .ReturnsAsync(new ApiResponse<DataResponse<List<NotificationEmail>>>(HttpStatusCode.OK, dataResponse));

        // act
        var response = await sut.CreateVacancyNotificationsAsync(id, VacancyStatus.Approved, CancellationToken.None);

        // assert
        response.Should().HaveCount(dataResponse.Data.Count);
        capturedRequest.Should().BeEquivalentTo(expectedRequest);
    }
}