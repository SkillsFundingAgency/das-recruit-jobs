using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;

public interface ITransferVacanciesToQaReviewHandler
{
    Task RunAsync(TransferVacanciesFromEmployerReviewToQaReviewQueueMessage message, CancellationToken cancellationToken);    
}

internal class TransferVacanciesToQaReviewHandler(IJobsOuterClient jobsOuterClient): ITransferVacanciesToQaReviewHandler
{
    public Task RunAsync(TransferVacanciesFromEmployerReviewToQaReviewQueueMessage message, CancellationToken cancellationToken)
    {
        // fetch the vacancies to transfer
        // send a message to transfer each one
    }
}