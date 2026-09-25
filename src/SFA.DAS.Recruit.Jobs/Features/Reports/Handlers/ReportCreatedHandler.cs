using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.Features.Reports.Handlers;

public interface IReportCreatedHandler
{
    Task RunAsync(Guid reportId, CancellationToken cancellationToken);
}

public class ReportCreatedHandler(IJobsOuterClient jobsOuterClient) : IReportCreatedHandler
{
    public async Task RunAsync(Guid reportId, CancellationToken cancellationToken)
    {
        await jobsOuterClient.PostAsync(new PostGenerateReportRequest(reportId), cancellationToken);
    }
}