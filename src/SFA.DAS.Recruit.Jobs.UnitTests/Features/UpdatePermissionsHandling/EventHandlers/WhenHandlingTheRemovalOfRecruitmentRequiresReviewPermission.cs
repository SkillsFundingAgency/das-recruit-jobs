using System.Net;
using SFA.DAS.ProviderRelationships.Messages.Events;
using SFA.DAS.ProviderRelationships.Types.Models;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.EventHandlers;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;
using SFA.DAS.Recruit.Jobs.OuterApi.Responses;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.UpdatePermissionsHandling.EventHandlers;

public class WhenHandlingTheRemovalOfRecruitmentRequiresReviewPermission
{
    [Test, MoqAutoData]
    public async Task Then_If_The_User_Reference_Is_Not_Set_Then_The_Message_Is_Not_Handled(
        IMessageHandlerContext context,
        [Frozen] Mock<IQueueClient<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage>> queueClient,
        [Greedy] UpdatedPermissionsExternalSystemEventsHandler sut)
    {
        // arrange
        var message = new UpdatedPermissionsEvent(123, 234, 345, 456, 567, null, string.Empty,
            string.Empty, string.Empty, [], [], DateTime.Now);

        // act
        await sut.Handle(message, context);

        // assert
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_The_Permission_Still_Exists_Then_The_Message_Is_Not_Handled(
        IMessageHandlerContext context,
        [Frozen] Mock<IQueueClient<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage>> queueClient,
        [Greedy] UpdatedPermissionsExternalSystemEventsHandler sut)
    {
        // arrange
        var message = new UpdatedPermissionsEvent(123, 234, 345, 456, 567, Guid.NewGuid(), string.Empty, string.Empty, string.Empty,
            [Operation.Recruitment, Operation.RecruitmentRequiresReview], [Operation.Recruitment, Operation.RecruitmentRequiresReview], DateTime.Now);

        // act
        await sut.Handle(message, context);

        // assert
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_The_Legal_Entity_Is_Not_Found_Then_The_Message_Is_Not_Handled(
        IMessageHandlerContext context,
        GetAccountLegalEntitiesResponse accountLegalEntitiesResponse,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] UpdatedPermissionsExternalSystemEventsHandler sut)
    {
        // arrange
        var message = new UpdatedPermissionsEvent(123, 234, 345, 456, 567, Guid.NewGuid(), "email address",
            "forename", "surname", [Operation.Recruitment], [Operation.Recruitment, Operation.RecruitmentRequiresReview], DateTime.Now);
        
        jobsOuterClient
            .Setup(x => x.GetAsync<GetAccountLegalEntitiesResponse>(It.IsAny<GetAccountLegalEntitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetAccountLegalEntitiesResponse>(HttpStatusCode.OK, accountLegalEntitiesResponse));

        // act
        var action = async () => await sut.Handle(message, context);

        // assert
        await action.Should().ThrowAsync<Exception>();
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_The_Permission_Is_Removed_Then_A_Queue_Message_Is_Sent(
        IMessageHandlerContext context,
        GetAccountLegalEntitiesResponse accountLegalEntitiesResponse,
        [Frozen] Mock<IQueueClient<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage>> queueClient,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] UpdatedPermissionsExternalSystemEventsHandler sut)
    {
        // arrange
        var message = new UpdatedPermissionsEvent(123, 234, 345, 456, 567, Guid.NewGuid(), "email address",
            "forename", "surname", [Operation.Recruitment], [Operation.Recruitment, Operation.RecruitmentRequiresReview], DateTime.Now);
        
        TransferVacanciesFromEmployerReviewToQaReviewQueueMessage? capturedMessage = null;
        queueClient
            .Setup(x => x.SendMessageAsync(It.IsAny<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage>(), It.IsAny<CancellationToken>()))
            .Callback<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage, CancellationToken>((x, _) => capturedMessage = x)
            .Returns(Task.CompletedTask);

        accountLegalEntitiesResponse.AccountLegalEntities[0].AccountLegalEntityId = message.AccountLegalEntityId;
        jobsOuterClient
            .Setup(x => x.GetAsync<GetAccountLegalEntitiesResponse>(It.IsAny<GetAccountLegalEntitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetAccountLegalEntitiesResponse>(HttpStatusCode.OK, accountLegalEntitiesResponse));

        // act
        await sut.Handle(message, context);

        // assert
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        capturedMessage.Should().NotBeNull();
        capturedMessage.Ukprn.Should().Be(567);
        capturedMessage.UserEmailAddress.Should().Be(message.UserEmailAddress);
        capturedMessage.UserRef.Should().Be(message.UserRef!.Value);
        capturedMessage.UserName.Should().Be($"{message.UserFirstName} {message.UserLastName}");
    }
}