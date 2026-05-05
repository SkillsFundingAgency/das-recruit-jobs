using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Features.VacanciesToClose.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using System.Net;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.VacanciesToClose.Handlers;

[TestFixture]
internal class WhenHandlingCloseExpiredVacancies
{
    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseIsNull(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] CloseExpiredVacanciesHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacanciesToCloseAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToClose>(HttpStatusCode.OK, new Jobs.OuterApi.Common.VacanciesToClose()));

        // Act
        await sut.RunAsync(CancellationToken.None);
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseFails(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] CloseExpiredVacanciesHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacanciesToCloseAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToClose>(HttpStatusCode.InternalServerError, new Jobs.OuterApi.Common.VacanciesToClose(), "Internal error"));

        // Act
        await sut.RunAsync(CancellationToken.None);
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_NoVacanciesFound(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] CloseExpiredVacanciesHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacanciesToCloseAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToClose>(HttpStatusCode.OK, new(), null));

        // Act
        await sut.RunAsync(CancellationToken.None);
    }
}
