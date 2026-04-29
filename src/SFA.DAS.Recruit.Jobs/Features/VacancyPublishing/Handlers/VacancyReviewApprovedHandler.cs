using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyPublishing.Handlers;

public interface IVacancyReviewApprovedHandler
{
    Task HandleAsync(Guid vacancyId, CancellationToken cancellationToken);
}
    
public class VacancyReviewApprovedReviewApprovedHandler(IJobsOuterClient jobsOuterClient): IVacancyReviewApprovedHandler
{
    public async Task HandleAsync(Guid vacancyId, CancellationToken cancellationToken)
    {
        var approveResponse = await jobsOuterClient.PostAsync(new PostApproveVacancyRequest(vacancyId), cancellationToken);
        approveResponse.ThrowIfErrored();
        
        var publishResponse = await jobsOuterClient.PostAsync(new PostPublishVacancyRequest(vacancyId), cancellationToken);
        publishResponse.ThrowIfErrored();
    }
}