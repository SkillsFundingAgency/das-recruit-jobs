using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyAnalyticsMigration;
public class VacancyAnalyticsMigrationStrategy(ILogger<VacancyAnalyticsMigrationStrategy> logger,
    VacancyAnalyticsMigrationMongoRepository mongoRepository,
    VacancyAnalyticsSqlRepository sqlRepository)
{
    private const int BatchSize = 200;
    private const int MaxRuntimeInSeconds = 270; // 4m 30s

    public async Task RunAsync()
    {
        var startTime = DateTime.UtcNow;
        var mongoAllVacancyAnalyticsSummariesAsync = await mongoRepository.GetAllVacancyAnalyticsSummariesAsync(BatchSize, startTime);
        while (mongoAllVacancyAnalyticsSummariesAsync is { Count: > 0 } && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            await ProcessBatchAsync(mongoAllVacancyAnalyticsSummariesAsync);
            mongoAllVacancyAnalyticsSummariesAsync = await mongoRepository.GetAllVacancyAnalyticsSummariesAsync(BatchSize, startTime);
        }
    }

    private async Task ProcessBatchAsync(List<VacancyAnalyticsSummaryV2> vacancyAnalyticsSummary)
    {
        var mappedVacancyAnalytics = MapVacancyAnalyticsFrom(vacancyAnalyticsSummary);

        if (mappedVacancyAnalytics is { Count: > 0 })
        {
            await sqlRepository.UpsertVacancyAnalyticsBatchAsync(mappedVacancyAnalytics.ToList());
            logger.LogInformation("Imported {count} VacancyAnalytics", mappedVacancyAnalytics.Count);
        }
    }

    private static List<VacancyAnalytics> MapVacancyAnalyticsFrom(List<VacancyAnalyticsSummaryV2> vacancyAnalyticsSummary)
    {
        var mappedRecords = vacancyAnalyticsSummary
            .Select(x => new VacancyAnalytics
            {
                VacancyReference = x.VacancyReference,
                UpdatedDate = x.LastUpdated,
                Analytics = System.Text.Json.JsonSerializer.Serialize(x.VacancyAnalytics)
            })
            .ToList();
        return mappedRecords;
    }
}