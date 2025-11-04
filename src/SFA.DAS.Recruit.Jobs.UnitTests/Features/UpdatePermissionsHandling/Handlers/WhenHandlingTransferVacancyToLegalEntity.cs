using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi.Clients;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.UpdatePermissionsHandling.Handlers;

internal class WhenHandlingTransferVacancyToLegalEntity
{
    [Test, MoqAutoData]
    public async Task Then_The_Api_Should_Be_Called_To_Transfer_The_Vacancy(
        TransferVacancyToLegalEntityQueueMessage message,
        [Frozen] Mock<IUpdatedPermissionsClient> updatePermissionsClient,
        [Greedy] TransferVacancyToLegalEntityHandler sut)
    {
        // act
        await sut.RunAsync(message, CancellationToken.None);

        // assert
        updatePermissionsClient.Verify(x => x.TransferVacancyAsync(
            It.Is<Guid>(y => y == message.VacancyId),
            It.Is<Guid>(y => y == message.UserRef),
            It.Is<string>(y => y == message.UserEmailAddress),
            It.Is<string>(y => y == message.UserName),
            It.Is<TransferReason>(y => y == message.TransferReason),
            CancellationToken.None
        ), Times.Once);
    }
}