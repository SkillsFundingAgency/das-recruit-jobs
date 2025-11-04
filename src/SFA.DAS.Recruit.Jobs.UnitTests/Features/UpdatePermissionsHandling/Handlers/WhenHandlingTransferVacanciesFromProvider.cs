using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi.Clients;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.UpdatePermissionsHandling.Handlers;

internal class WhenHandlingTransferVacanciesFromProvider
{
    [Test, MoqAutoData]
    public async Task Then_The_Correct_Messages_Should_Be_Added_To_The_Queue(
        List<Guid> vacancyIds,
        TransferVacanciesFromProviderQueueMessage message,
        [Frozen] Mock<IUpdatedPermissionsClient> updatePermissionsClient,
        [Frozen] Mock<IQueueClient<TransferVacancyToLegalEntityQueueMessage>> queueClient,
        [Greedy] TransferVacanciesFromProviderHandler sut)
    {
        // arrange
        updatePermissionsClient
            .Setup(x => x.GetProviderVacanciesToTransfer(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vacancyIds);

        List<TransferVacancyToLegalEntityQueueMessage> capturedMessages = [];
        queueClient
            .Setup(x => x.SendMessageAsync(It.IsAny<TransferVacancyToLegalEntityQueueMessage>()))
            .Callback((TransferVacancyToLegalEntityQueueMessage x) => capturedMessages.Add(x))
            .Returns(Task.CompletedTask);

        // act
        await sut.RunAsync(message, CancellationToken.None);

        // assert
        updatePermissionsClient.Verify(x => x.GetProviderVacanciesToTransfer(
            It.Is<long>(y => y == message.Ukprn),
            It.Is<long>(y => y == message.EmployerAccountId),
            It.Is<long>(y => y == message.AccountLegalEntityId),
            CancellationToken.None
        ), Times.Once);
        
        capturedMessages.Count.Should().Be(vacancyIds.Count);
        capturedMessages.Should().AllSatisfy(x =>
        {
            vacancyIds.Should().Contain(x.VacancyId);
            x.UserRef.Should().Be(message.UserRef);
            x.UserName.Should().Be(message.UserName);
            x.UserEmailAddress.Should().Be(message.UserEmailAddress);
            x.TransferReason.Should().Be(message.TransferReason);
        });
    }
    
    [Test, MoqAutoData]
    public async Task Then_When_There_Are_No_Vacancies_To_Transfer_No_Messages_Should_Be_Added_To_The_Queue(
        TransferVacanciesFromProviderQueueMessage message,
        [Frozen] Mock<IUpdatedPermissionsClient> updatePermissionsClient,
        [Frozen] Mock<IQueueClient<TransferVacancyToLegalEntityQueueMessage>> queueClient,
        [Greedy] TransferVacanciesFromProviderHandler sut)
    {
        // arrange
        updatePermissionsClient
            .Setup(x => x.GetProviderVacanciesToTransfer(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // act
        await sut.RunAsync(message, CancellationToken.None);

        // assert
        updatePermissionsClient.Verify(x => x.GetProviderVacanciesToTransfer(
            It.Is<long>(y => y == message.Ukprn),
            It.Is<long>(y => y == message.EmployerAccountId),
            It.Is<long>(y => y == message.AccountLegalEntityId),
            CancellationToken.None), Times.Once);
        
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<TransferVacancyToLegalEntityQueueMessage>()), Times.Never);
    }
}