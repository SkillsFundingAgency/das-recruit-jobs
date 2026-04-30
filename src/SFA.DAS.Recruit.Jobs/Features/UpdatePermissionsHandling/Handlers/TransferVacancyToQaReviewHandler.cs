using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;

public interface ITransferVacancyToQaReviewHandler
{
    Task RunAsync(TransferVacancyFromEmployerReviewToQaReviewQueueMessage message, CancellationToken cancellationToken);    
}

internal class TransferVacancyToQaReviewHandler(IJobsOuterClient jobsOuterClient): ITransferVacancyToQaReviewHandler
{
    public async Task RunAsync(TransferVacancyFromEmployerReviewToQaReviewQueueMessage message, CancellationToken cancellationToken)
    {
    }
}