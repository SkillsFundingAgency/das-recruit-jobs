using SFA.DAS.Recruit.Jobs.Features.Reports.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.GenerateReportHandling.Handlers;

public class WhenHandlingReportCreated
{
    [Test, MoqAutoData]
    public async Task Then_The_Generate_Report_Request_Is_Sent_To_The_Outer_Api(
        Guid reportId,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] ReportCreatedHandler sut)
    {
        // act
        await sut.RunAsync(reportId, CancellationToken.None);

        // assert
        jobsOuterClient.Verify(x => x.PostAsync(It.Is<PostGenerateReportRequest>(r => r.Url.Equals($"reports/generate/{reportId}")), It.IsAny<CancellationToken>()), Times.Once);
    }
}