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
    IQueueClient<TransferVacanciesFromProviderQueueMessage> queueClient): ITransferVacanciesFromProviderHandler
{
    public async Task RunAsync(QueueItem<TransferVacanciesFromProviderQueueMessage> message, CancellationToken cancellationToken)
    {
    }
}