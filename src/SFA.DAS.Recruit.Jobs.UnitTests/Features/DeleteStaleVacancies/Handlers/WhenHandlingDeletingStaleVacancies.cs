using System.Net;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using SFA.DAS.Recruit.Jobs.Features.DeleteStaleVacancies.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.DeleteStaleVacancies.Handlers;

[TestFixture]
internal class WhenHandlingDeletingStaleVacancies
{
    [Test, MoqAutoData]
    public async Task RunAsync_Should_Delete_Vacancies_For_StaleVacancies_WhenResponseIsValid(
        StaleVacancies draftStaleVacanciesResponse,
        StaleVacancies referredStaleVacanciesResponse,
        StaleVacancies employerRejectedStaleVacanciesResponse,
        StaleVacancies qaRejectedStaleVacanciesResponse,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DeleteStaleVacanciesHandler sut)
    {
        // Arrange
        foreach (var staleVacancyToClose in draftStaleVacanciesResponse.Data)
        {
            staleVacancyToClose.Status = VacancyStatus.Draft;
        }
        foreach (var staleVacancyToClose in referredStaleVacanciesResponse.Data)
        {
            staleVacancyToClose.Status = VacancyStatus.Review;
        }
        foreach (var staleVacancyToClose in employerRejectedStaleVacanciesResponse.Data)
        {
            staleVacancyToClose.Status = VacancyStatus.Rejected;
        }
        foreach (var staleVacancyToClose in qaRejectedStaleVacanciesResponse.Data)
        {
            staleVacancyToClose.Status = VacancyStatus.Referred;
        }
        jobsOuterClient
            .Setup(x => x.GetDraftVacanciesToCloseAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, draftStaleVacanciesResponse));
        jobsOuterClient
            .Setup(x => x.GetEmployerReviewedVacanciesToClose(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, referredStaleVacanciesResponse));
        jobsOuterClient
            .Setup(x => x.GetEmployerRejectedVacanciesToClose(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, employerRejectedStaleVacanciesResponse));
        jobsOuterClient
            .Setup(x => x.GetQaRejectedVacanciesToClose(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, qaRejectedStaleVacanciesResponse));

        jobsOuterClient.Setup(x => x.DeleteVacancyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse(HttpStatusCode.NoContent));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        var totalVacancies = (draftStaleVacanciesResponse.Data?.Count() ?? 0)
            + (referredStaleVacanciesResponse.Data?.Count() ?? 0)
            + (employerRejectedStaleVacanciesResponse.Data?.Count() ?? 0)
            + (qaRejectedStaleVacanciesResponse.Data?.Count() ?? 0);
        jobsOuterClient.Verify(
            x => x.DeleteVacancyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Exactly(totalVacancies));
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Never_Delete_Vacancies_For_StaleVacancies_WhenResponseIsInValid(
        StaleVacancies draftStaleVacanciesResponse,
        StaleVacancies referredStaleVacanciesResponse,
        StaleVacancies employerRejectedStaleVacanciesResponse,
        StaleVacancies qaRejectedStaleVacanciesResponse,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DeleteStaleVacanciesHandler sut)
    {
        // Arrange
        foreach (var staleVacancyToClose in draftStaleVacanciesResponse.Data)
        {
            staleVacancyToClose.Status = VacancyStatus.Live;
        }
        foreach (var staleVacancyToClose in referredStaleVacanciesResponse.Data)
        {
            staleVacancyToClose.Status = VacancyStatus.Live;
        }
        foreach (var staleVacancyToClose in employerRejectedStaleVacanciesResponse.Data)
        {
            staleVacancyToClose.Status = VacancyStatus.Live;
        }
        foreach (var staleVacancyToClose in qaRejectedStaleVacanciesResponse.Data)
        {
            staleVacancyToClose.Status = VacancyStatus.Live;
        }
        jobsOuterClient
            .Setup(x => x.GetDraftVacanciesToCloseAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, draftStaleVacanciesResponse));
        jobsOuterClient
            .Setup(x => x.GetEmployerReviewedVacanciesToClose(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, referredStaleVacanciesResponse));
        jobsOuterClient
            .Setup(x => x.GetEmployerRejectedVacanciesToClose(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, employerRejectedStaleVacanciesResponse));
        jobsOuterClient
            .Setup(x => x.GetQaRejectedVacanciesToClose(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, qaRejectedStaleVacanciesResponse));


        jobsOuterClient.Setup(x => x.DeleteVacancyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse(HttpStatusCode.NoContent));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(
            x => x.DeleteVacancyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Never_Delete_Vacancies_For_StaleVacancies_WhenResponseIsEmpty(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] DeleteStaleVacanciesHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetDraftVacanciesToCloseAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, new StaleVacancies()));
        jobsOuterClient
            .Setup(x => x.GetEmployerReviewedVacanciesToClose(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, new StaleVacancies()));
        jobsOuterClient
            .Setup(x => x.GetEmployerRejectedVacanciesToClose(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, new StaleVacancies()));
        jobsOuterClient
            .Setup(x => x.GetQaRejectedVacanciesToClose(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<StaleVacancies>(HttpStatusCode.OK, new StaleVacancies()));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(
            x => x.DeleteVacancyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}