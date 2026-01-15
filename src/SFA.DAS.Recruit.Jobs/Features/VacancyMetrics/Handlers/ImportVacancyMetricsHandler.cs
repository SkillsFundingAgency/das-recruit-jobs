using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using SFA.DAS.Recruit.Jobs.Features.VacancyAnalyticsMigration;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Metrics;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMetrics.Handlers;

public interface IImportVacancyMetricsHandler
{
    Task RunAsync(CancellationToken cancellationToken);
}

public class ImportVacancyMetricsHandler(ILogger<ImportVacancyMetricsHandler> logger,
    VacancyAnalyticsSqlRepository vacancyAnalyticsSqlRepository,
    IRecruitJobsOuterClient recruitJobsOuterClient) : IImportVacancyMetricsHandler
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var canContinue = true;
        while (!cancellationToken.IsCancellationRequested && canContinue)
        {
            canContinue = await UpsertVacancyMetrics(cancellationToken);
        }
    }

    private async Task<bool> UpsertVacancyMetrics(CancellationToken cancellationToken)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddHours(-1);

        var response = await recruitJobsOuterClient
            .GetVacancyMetricsByDateAsync(startDate, endDate, cancellationToken);

        if (!response.Success || response.Payload?.VacancyMetrics is not { Count: > 0 })
        {
            logger.LogInformation("No records found from outer api while fetching vacancy metrics by date. StartDate:{startDate} and EndDate:{endDate}", startDate, endDate);
            return false;
        }

        var vacancyMetricsEntity = new List<VacancyAnalytics>(response.Payload.VacancyMetrics.Count);

        foreach (var vacancyMetric in response.Payload.VacancyMetrics)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existingMetrics =
                await vacancyAnalyticsSqlRepository
                    .FindVacancyAnalyticsByVacancyReference(vacancyMetric.VacancyRef);

            var aggregatedAnalytics = AggregateAnalytics(
                startDate,
                existingMetrics?.AnalyticsData,
                vacancyMetric);

            vacancyMetricsEntity.Add(MapVacancyAnalyticsFrom(vacancyMetric.VacancyRef, aggregatedAnalytics));
        }

        logger.LogInformation("Upserting {count} vacancy metrics information", vacancyMetricsEntity.Count);
        await vacancyAnalyticsSqlRepository
            .UpsertVacancyAnalyticsBatchAsync(vacancyMetricsEntity, cancellationToken);

        return true;
    }

    private static Core.Models.VacancyAnalytics AggregateAnalytics(
        DateTime analyticsDate,
        Core.Models.VacancyAnalytics? existing,
        VacancyMetricResponse.VacancyMetric vacancyMetric)
    {
        return new Core.Models.VacancyAnalytics
        {
            AnalyticsDate = analyticsDate,
            ApplicationStartedCount =
                (existing?.ApplicationStartedCount ?? 0) + vacancyMetric.ApplicationStartedCount,

            ViewsCount =
                (existing?.ViewsCount ?? 0) + vacancyMetric.ViewsCount,

            ApplicationSubmittedCount =
                (existing?.ApplicationSubmittedCount ?? 0) + vacancyMetric.ApplicationSubmittedCount,

            SearchResultsCount =
                (existing?.SearchResultsCount ?? 0) + vacancyMetric.SearchResultsCount
        };
    }

    private static VacancyAnalytics MapVacancyAnalyticsFrom(long vacancyReference, Core.Models.VacancyAnalytics vacancyAnalytics)
    {
        return new VacancyAnalytics
        {
            VacancyReference = vacancyReference,
            UpdatedDate = DateTime.UtcNow,
            Analytics = vacancyAnalytics.ToJson()
        };
    }
}