using AutoFixture.NUnit3;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Models;
using SFA.DAS.Recruit.Jobs.Features.VacancyMetrics.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Analytics;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Metrics;
using System.Net;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.Vacancies;

[TestFixture]
internal class WhenHandingImportVacancyAnalytics
{
    [Test, MoqAutoData]
    public async Task RunAsync_Should_Put_VacancyAnalytics_For_Metrics_Data_WhenResponseIsValid(long vacancyReference,
        VacancyMetricResponse metricResponse,
        GetOneVacancyAnalyticsResponse vacancyAnalyticsResponse,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] ImportVacancyMetricsHandler sut)
    {
        // Arrange
        foreach (var metric in metricResponse.VacancyMetrics)
        {
            metric.VacancyReference = vacancyReference.ToString();
        }
        vacancyAnalyticsResponse.VacancyReference = vacancyReference;
        
        jobsOuterClient
            .Setup(x => x.GetVacancyMetricsByDateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<VacancyMetricResponse>(true, HttpStatusCode.OK, metricResponse, null));

        jobsOuterClient
            .Setup(x => x.GetOneVacancyAnalyticsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOneVacancyAnalyticsResponse>(true, HttpStatusCode.OK, vacancyAnalyticsResponse, null));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(x => x.PutOneVacancyAnalyticsAsync(It.IsAny<long>(), It.IsAny<List<VacancyAnalytics>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseIsNull([Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] ImportVacancyMetricsHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacancyMetricsByDateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<VacancyMetricResponse>(true, HttpStatusCode.OK, new VacancyMetricResponse(), null));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(x => x.PutOneVacancyAnalyticsAsync(It.IsAny<long>(), It.IsAny<List<VacancyAnalytics>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseFails([Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Greedy] ImportVacancyMetricsHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacancyMetricsByDateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<VacancyMetricResponse>(true, HttpStatusCode.InternalServerError, new VacancyMetricResponse(), "Internal error"));

        // Act
        await sut.RunAsync(CancellationToken.None);

        // Assert
        jobsOuterClient.Verify(x => x.PutOneVacancyAnalyticsAsync(It.IsAny<long>(), It.IsAny<List<VacancyAnalytics>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}