using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi.Clients;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;

public interface ITransferVacanciesFromProviderHandler
{
    Task RunAsync(TransferVacanciesFromProviderQueueMessage message, CancellationToken cancellationToken);    
}

internal class TransferVacanciesFromProviderHandler(
    IUpdatedPermissionsClient updatePermissionsClient,
    IQueueClient<TransferVacancyToLegalEntityQueueMessage> queueClient): ITransferVacanciesFromProviderHandler
{
    public async Task RunAsync(TransferVacanciesFromProviderQueueMessage message, CancellationToken cancellationToken)
    {
        var vacancies = await updatePermissionsClient.GetProviderVacanciesToTransfer(message.Ukprn, message.EmployerAccountId, message.AccountLegalEntityId, cancellationToken);
        var tasks = vacancies.Select(x => queueClient.SendMessageAsync(new TransferVacancyToLegalEntityQueueMessage
        {
            VacancyId = x,
            UserRef = message.UserRef,
            UserEmailAddress = message.UserEmailAddress,
            UserName = message.UserName,
            TransferReason = message.TransferReason,
        }));

        await Task.WhenAll(tasks);
    }
}