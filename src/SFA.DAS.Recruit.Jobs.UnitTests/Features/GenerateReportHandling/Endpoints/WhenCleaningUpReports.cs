using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Recruit.Jobs.Features.Reports.Endpoints;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.GenerateReportHandling.Endpoints;

public class WhenCleaningUpReports
{
    [Test, MoqAutoData]
    public async Task Then_The_Outer_Api_Call_Is_Made_To_Perform_The_Cleanup(
        TimerInfo timerInfo,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] ReportsCleanupTimerTrigger sut)
    {
        // act
        await sut.Run(timerInfo, CancellationToken.None);

        // assert
        jobsOuterClient.Verify(x => x.PostAsync(It.IsAny<PostReportsCleanupRequest>(), CancellationToken.None), Times.Once);
    }
}