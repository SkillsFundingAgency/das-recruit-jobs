using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.OuterApi;
using static SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Metrics.VacancyMetricResponse;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMetrics.Handlers;

public interface IImportVacancyMetricsHandler
{
    Task RunAsync(CancellationToken cancellationToken);
}

public class ImportVacancyMetricsHandler(ILogger<ImportVacancyMetricsHandler> logger,
    IRecruitJobsOuterClient recruitJobsOuterClient) : IImportVacancyMetricsHandler
{
    private const int MaxDegreeOfParallelism = 10; // tune based on API limits

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await UpsertVacancyMetrics(cancellationToken);
    }

    private async Task UpsertVacancyMetrics(CancellationToken cancellationToken)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddHours(-1);

            var response = await recruitJobsOuterClient
                .GetVacancyMetricsByDateAsync(startDate, endDate, cancellationToken);

            var vacancyMetrics = response.Payload?.VacancyMetrics;

            if (!response.Success || vacancyMetrics is not { Count: > 0 })
            {
                logger.LogInformation("No records found from outer api while fetching vacancy metrics by date. StartDate:{startDate} and EndDate:{endDate}", startDate, endDate);
                return;
            }

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(vacancyMetrics, parallelOptions,
                async (vacancyMetric, ct) =>
                {
                    var existingResponse =
                        await recruitJobsOuterClient.GetOneVacancyAnalyticsAsync(
                            vacancyMetric.VacancyRef,
                            ct);

                    var analyticsToUpsert = new List<Core.Models.VacancyAnalytics>();

                    if (existingResponse is { Success: true, Payload: not null })
                    {
                        // Copy existing analytics first
                        analyticsToUpsert.AddRange(existingResponse.Payload.Analytics);

                        var index = analyticsToUpsert.FindIndex(x => x.AnalyticsDate == startDate);

                        if (index >= 0)
                        {
                            // Replace existing analytic for the same date
                            analyticsToUpsert[index] = Merge(analyticsToUpsert[index], vacancyMetric, startDate);
                        }
                        else
                        {
                            // Append new analytic
                            analyticsToUpsert.Add(CreateNew(vacancyMetric, startDate));
                        }
                    }
                    else
                    {
                        analyticsToUpsert.Add(CreateNew(vacancyMetric, startDate));
                    }

                    await recruitJobsOuterClient.PutOneVacancyAnalyticsAsync(vacancyMetric.VacancyRef,
                        analyticsToUpsert,
                        ct);

                    cancellationToken.ThrowIfCancellationRequested();
                });

            logger.LogInformation(
                "Upserted {Count} vacancy metrics records",
                vacancyMetrics.Count);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Import vacancy metrics operation was cancelled.");
        }
    }

    private static Core.Models.VacancyAnalytics CreateNew(
        VacancyMetric vacancyMetric,
        DateTime analyticsDate) =>
        new()
        {
            AnalyticsDate = analyticsDate,
            ApplicationStartedCount = vacancyMetric.ApplicationStartedCount,
            ApplicationSubmittedCount = vacancyMetric.ApplicationSubmittedCount,
            SearchResultsCount = vacancyMetric.SearchResultsCount,
            ViewsCount = vacancyMetric.ViewsCount
        };

    private static Core.Models.VacancyAnalytics Merge(
        Core.Models.VacancyAnalytics existing,
        VacancyMetric incoming,
        DateTime analyticsDate) =>
        new()
        {
            AnalyticsDate = analyticsDate,
            ApplicationStartedCount =
                existing.ApplicationStartedCount + incoming.ApplicationStartedCount,
            ApplicationSubmittedCount =
                existing.ApplicationSubmittedCount + incoming.ApplicationSubmittedCount,
            SearchResultsCount =
                existing.SearchResultsCount + incoming.SearchResultsCount,
            ViewsCount =
                existing.ViewsCount + incoming.ViewsCount
        };
}