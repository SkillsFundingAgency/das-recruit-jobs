using SFA.DAS.ProviderRelationships.Messages.Events;
using SFA.DAS.ProviderRelationships.Types.Models;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.EventHandlers;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi.Clients;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.UpdatePermissionsHandling.EventHandlers;

public class WhenHandlingUpdatedPermissionsExternalSystemEvents
{
    [Test, MoqAutoData]
    public async Task Then_If_The_User_Reference_Is_Not_Set_Then_The_Message_Is_Not_Handled(
        IMessageHandlerContext context,
        [Frozen] Mock<IQueueClient<TransferVacanciesFromProviderQueueMessage>> queueClient,
        [Greedy] UpdatedPermissionsExternalSystemEventsHandler sut)
    {
        // arrange
        var message = new UpdatedPermissionsEvent(123, 234, 345, 456, 567, null, string.Empty,
            string.Empty, string.Empty, [Operation.Recruitment], [Operation.Recruitment], DateTime.Now);

        // act
        await sut.Handle(message, context);

        // assert
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<TransferVacanciesFromProviderQueueMessage>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_The_Permission_Still_Exists_Then_The_Message_Is_Not_Handled(
        IMessageHandlerContext context,
        [Frozen] Mock<IQueueClient<TransferVacanciesFromProviderQueueMessage>> queueClient,
        [Greedy] UpdatedPermissionsExternalSystemEventsHandler sut)
    {
        // arrange
        var message = new UpdatedPermissionsEvent(123, 234, 345, 456, 567, Guid.NewGuid(), string.Empty,
            string.Empty, string.Empty, [Operation.Recruitment], [Operation.Recruitment], DateTime.Now);

        // act
        await sut.Handle(message, context);

        // assert
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<TransferVacanciesFromProviderQueueMessage>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_The_Permission_Is_Removed_Then_A_Queue_Message_Is_Sent(
        IMessageHandlerContext context,
        [Frozen] Mock<IUpdatedPermissionsClient> updatePermissionsClient,
        [Frozen] Mock<IQueueClient<TransferVacanciesFromProviderQueueMessage>> queueClient,
        [Greedy] UpdatedPermissionsExternalSystemEventsHandler sut)
    {
        // arrange
        var message = new UpdatedPermissionsEvent(123, 234, 345, 456, 567, Guid.NewGuid(), "email address",
            "forename", "surname", [], [Operation.Recruitment], DateTime.Now);
        
        TransferVacanciesFromProviderQueueMessage? capturedMessage = null;
        queueClient
            .Setup(x => x.SendMessageAsync(It.IsAny<TransferVacanciesFromProviderQueueMessage>()))
            .Callback<TransferVacanciesFromProviderQueueMessage>(x => capturedMessage = x)
            .Returns(Task.CompletedTask);

        // act
        await sut.Handle(message, context);

        // assert
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<TransferVacanciesFromProviderQueueMessage>()), Times.Once);
        capturedMessage.Should().NotBeNull();
        capturedMessage.AccountLegalEntityId.Should().Be(234);
        capturedMessage.Ukprn.Should().Be(567);
        capturedMessage.TransferReason.Should().Be(TransferReason.EmployerRevokedPermission);
    }
}