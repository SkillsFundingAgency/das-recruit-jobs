using System.Net;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;
using SFA.DAS.Recruit.Jobs.OuterApi.Responses;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.UpdatePermissionsHandling.Handlers;

public class WhenHandlingTransferVacanciesToQaReview
{
    [Test, MoqAutoData]
    public async Task Then_The_Events_Are_Queued(
        long accountLegalEntityId,
        TransferVacanciesFromEmployerReviewToQaReviewQueueMessage message,
        GetProviderOwnedVacanciesInReviewResponse response,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IQueueClient<TransferVacancyFromEmployerReviewToQaReviewQueueMessage>> queueClient,
        [Frozen] Mock<IEncodingService> encodingService,
        [Greedy] TransferVacanciesToQaReviewHandler sut,
        CancellationToken cancellationToken)
    {
        // arrange
        GetProviderOwnedVacanciesInReviewRequest? capturedRequest = null;
        jobsOuterClient
            .Setup(x => x.GetAsync<GetProviderOwnedVacanciesInReviewResponse>(It.IsAny<IGetRequest>(), cancellationToken))
            .Callback<IGetRequest, CancellationToken>((x, _) => capturedRequest = x as GetProviderOwnedVacanciesInReviewRequest)
            .ReturnsAsync(new ApiResponse<GetProviderOwnedVacanciesInReviewResponse>(HttpStatusCode.OK, response));
        
        encodingService
            .Setup(x => x.Decode(message.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId))
            .Returns(accountLegalEntityId);

        // act
        await sut.RunAsync(message, cancellationToken);

        // assert
        capturedRequest.Should().NotBeNull();
        capturedRequest.Url.Should().Be(new GetProviderOwnedVacanciesInReviewRequest(message.Ukprn, accountLegalEntityId).Url);
        foreach (var vacancyId in response)
        {
            queueClient.Verify(x => x.SendMessageAsync(It.Is<TransferVacancyFromEmployerReviewToQaReviewQueueMessage>(m => 
                m.VacancyId == vacancyId
                && m.UserName == message.UserName 
                && m.UserReference == message.UserRef
                && m.UserEmailAddress == message.UserEmailAddress), cancellationToken), Times.Once);    
        }
    }
}