using System.Net;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.UpdatePermissionsHandling.Handlers;

public class WhenHandlingTransferVacancyToQaReview
{
    [Test, MoqAutoData]
    public async Task Then_The_Request_To_Transfer_The_Vacancy_Is_Sent(
        TransferVacancyFromEmployerReviewToQaReviewQueueMessage message,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] TransferVacancyToQaReviewHandler sut)
    {
        // arrange
        PostTransferVacancyToQaReviewRequest? capturedRequest = null;
        jobsOuterClient
            .Setup(x => x.PostAsync(It.IsAny<IPostRequest>(), CancellationToken.None))
            .Callback<IPostRequest, CancellationToken>((x, _) => capturedRequest = x as PostTransferVacancyToQaReviewRequest)
            .ReturnsAsync(new ApiResponse(HttpStatusCode.OK));

        // act
        await sut.RunAsync(message, CancellationToken.None);

        // assert
        capturedRequest.Should().NotBeNull();
        capturedRequest.Url.Should().Be(new PostTransferVacancyToQaReviewRequest(message.VacancyId, message.UserReference, message.UserEmailAddress).Url);
        capturedRequest.Data.Should().BeNull();
    }
}