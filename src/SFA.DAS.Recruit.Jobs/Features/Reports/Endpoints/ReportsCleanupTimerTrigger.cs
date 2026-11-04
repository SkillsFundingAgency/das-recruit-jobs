using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Recruit.Jobs.Core;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.Features.Reports.Endpoints;

public class ReportsCleanupTimerTrigger(IJobsOuterClient jobsOuterClient)
{
    [Function(nameof(ReportsCleanupTimerTrigger))]
    public async Task Run([TimerTrigger(Schedules.FourAmDaily)] TimerInfo timerInfo, CancellationToken cancellationToken = default)
    {
        await jobsOuterClient.PostAsync(new PostReportsCleanupRequest(), cancellationToken);
    }
}