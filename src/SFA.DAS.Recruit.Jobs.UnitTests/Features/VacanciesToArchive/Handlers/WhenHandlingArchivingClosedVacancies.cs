using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using System.Net;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.VacanciesToArchive.Handlers;

[TestFixture]
internal class WhenHandlingArchivingClosedVacancies
{
    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseIsNull(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] ArchiveClosedVacanciesHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacanciesToArchiveAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToArchive>(HttpStatusCode.OK, new Jobs.OuterApi.Common.VacanciesToArchive()));

        // Act
        await sut.RunAsync(CancellationToken.None);
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseFails(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] ArchiveClosedVacanciesHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacanciesToArchiveAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToArchive>(HttpStatusCode.InternalServerError, new Jobs.OuterApi.Common.VacanciesToArchive(), "Internal error"));

        // Act
        await sut.RunAsync(CancellationToken.None);
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_NoVacanciesFound(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] ArchiveClosedVacanciesHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacanciesToArchiveAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToArchive>(HttpStatusCode.OK, new(), null));

        // Act
        await sut.RunAsync(CancellationToken.None);
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseIsNotNull(
        Jobs.OuterApi.Common.VacanciesToArchive response,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] ArchiveClosedVacanciesHandler sut)
    {
        // Arrange
        foreach (var vacancy in response.Data)
        {
            vacancy.ClosingDate = DateTime.UtcNow.AddDays(-190);
            vacancy.Status = VacancyStatus.Closed;
        }
        jobsOuterClient
            .Setup(x => x.GetVacanciesToArchiveAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToArchive>(HttpStatusCode.OK, response));
        jobsOuterClient
            .Setup(x => x.PostVacancyToArchive(It.IsAny<Guid>(), It.IsAny<long>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse(HttpStatusCode.OK, null));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(x => x.PostVacancyToArchive(It.IsAny<Guid>(), It.IsAny<long>(), CancellationToken.None), Times.Exactly(response.Data.Count()));
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseIsNotNull_Status_Not_Closed(
        Jobs.OuterApi.Common.VacanciesToArchive response,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] ArchiveClosedVacanciesHandler sut)
    {
        // Arrange
        foreach (var vacancy in response.Data)
        {
            vacancy.ClosingDate = DateTime.UtcNow.AddDays(-190);
            vacancy.Status = VacancyStatus.Live;
        }
        jobsOuterClient
            .Setup(x => x.GetVacanciesToArchiveAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToArchive>(HttpStatusCode.OK, response));
        
        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(x => x.PostVacancyToArchive(It.IsAny<Guid>(), It.IsAny<long>(), CancellationToken.None), Times.Never());
    }

    [Test]
    [MoqInlineAutoData(-1)]
    [MoqInlineAutoData(-50)]
    [MoqInlineAutoData(-181)]
    public async Task RunAsync_Should_Return_When_GetResponseIsNotNull_ClosingDate_Not_Less_Than_DefinedDate(int defaultArchiveStaleByDays,
        Jobs.OuterApi.Common.VacanciesToArchive response,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] ArchiveClosedVacanciesHandler sut)
    {
        // Arrange
        foreach (var vacancy in response.Data)
        {
            vacancy.ClosingDate = DateTime.UtcNow.AddDays(defaultArchiveStaleByDays);
            vacancy.Status = VacancyStatus.Closed;
        }
        jobsOuterClient
            .Setup(x => x.GetVacanciesToArchiveAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToArchive>(HttpStatusCode.OK, response));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(x => x.PostVacancyToArchive(It.IsAny<Guid>(), It.IsAny<long>(), CancellationToken.None), Times.Never());
    }
}