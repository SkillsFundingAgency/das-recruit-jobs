using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi.Clients;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;

public interface ITransferVacanciesFromProviderHandler
{
    Task RunAsync(QueueItem<TransferVacanciesFromProviderQueueMessage> message, CancellationToken cancellationToken);    
}

internal class TransferVacanciesFromProviderHandler(
    IUpdatedPermissionsClient updatePermissionsClient,
    IQueueClient<TransferVacancyToLegalEntityQueueMessage> queueClient): ITransferVacanciesFromProviderHandler
{
    public async Task RunAsync(QueueItem<TransferVacanciesFromProviderQueueMessage> message, CancellationToken cancellationToken)
    {
        var request = message.Payload;
        var vacancies = await updatePermissionsClient.GetProviderVacanciesToTransfer(request.Ukprn, request.EmployerAccountId, request.AccountLegalEntityPublicHashedId, cancellationToken);
        var tasks = vacancies.Select(x => queueClient.SendMessageAsync(new TransferVacancyToLegalEntityQueueMessage()
        {
            VacancyReference = x,
            UserRef = request.UserRef,
            UserEmailAddress = request.UserEmailAddress,
            UserName = request.UserName,
            TransferReason = request.TransferReason,
        }));

        await Task.WhenAll(tasks);
    }
}