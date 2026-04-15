using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using MongoReport = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.Report;
using SqlReport = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.Report;

namespace SFA.DAS.Recruit.Jobs.Features.QaReports;

[ExcludeFromCodeCoverage]
public class QaReportsMigrationStrategy(
    ILogger<QaReportsMigrationStrategy> logger,
    ReportMapper mapper,
    QaReportsMigrationMongoRepository mongoRepository,
    QaReportsMigrationSqlRepository sqlRepository)
{
    private const int BatchSize = 200;
    private const int MaxRuntimeInSeconds = 270; // 4m 30s

    public async Task RunAsync()
    {
        var startTime = DateTime.UtcNow;
        var remigrateIfBeforeDate = new DateTime(2025, 01, 01);
        var mongoReports = await mongoRepository.FetchBatchAsync(BatchSize, remigrateIfBeforeDate);
        while (mongoReports is { Count: > 0 } && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            await ProcessBatchAsync(mongoReports);
            mongoReports = await mongoRepository.FetchBatchAsync(BatchSize, remigrateIfBeforeDate);
        }
    }

    private async Task ProcessBatchAsync(List<MongoReport> reports)
    {
        List<MongoReport> failed = [];
        List<SqlReport> mapped = [];

        foreach (var report in reports)
        {
            try
            {
                mapped.Add(mapper.MapFrom(report));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to map Report {ReportId}", report.Id);
                failed.Add(report);
            }
        }

        if (failed is { Count: > 0 })
        {
            await mongoRepository.UpdateFailedMigrationDateBatchAsync(failed.Select(x => x.Id).ToList());
            logger.LogInformation("Failed to migrate {FailedCount} Reports", failed.Count);
        }

        if (mapped is { Count: > 0 })
        {
            await sqlRepository.UpsertReportsBatchAsync(mapped);
            logger.LogInformation("Imported {Count} Reports", mapped.Count);

            await mongoRepository.UpdateSuccessMigrationDateBatchAsync(mapped.Select(x => x.Id).ToList());
            logger.LogInformation("Marked {SuccessCount} Reports as migrated", mapped.Count);
        }
    }
}
