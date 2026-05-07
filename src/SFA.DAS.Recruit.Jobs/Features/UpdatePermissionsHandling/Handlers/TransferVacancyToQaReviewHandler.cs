using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;

public interface ITransferVacancyToQaReviewHandler
{
    Task RunAsync(TransferVacancyFromEmployerReviewToQaReviewQueueMessage message, CancellationToken cancellationToken);    
}

public class TransferVacancyToQaReviewHandler(IJobsOuterClient jobsOuterClient): ITransferVacancyToQaReviewHandler
{
    public async Task RunAsync(TransferVacancyFromEmployerReviewToQaReviewQueueMessage message, CancellationToken cancellationToken)
    {
        var request = new PostTransferVacancyToQaReviewRequest(message.VacancyId, message.UserReference, message.UserEmailAddress);
        var response = await jobsOuterClient.PostAsync(request, cancellationToken);
        response.ThrowIfErrored($"Failed to transfer vacancy '{message.VacancyId}' to QA review");
    }
}