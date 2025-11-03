using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi.Clients;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;

public interface ITransferVacancyToLegalEntityHandler
{
    Task RunAsync(TransferVacancyToLegalEntityQueueMessage message, CancellationToken cancellationToken);    
}

internal class TransferVacancyToLegalEntityHandler(IUpdatedPermissionsClient updatePermissionsClient): ITransferVacancyToLegalEntityHandler
{
    public async Task RunAsync(TransferVacancyToLegalEntityQueueMessage message, CancellationToken cancellationToken)
    {
        await updatePermissionsClient.TransferVacancyAsync(message.VacancyReference, message.UserRef, message.UserEmailAddress, message.UserName, message.TransferReason, cancellationToken);
    }
}