using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core.Services;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

public class ApplicationReviewMigrationStrategy(
    ILogger<ApplicationReviewMigrationStrategy> logger,
    ApplicationReviewsMigrationMongoRepository mongoRepository,
    ApplicationReviewsMigrationSqlRepository sqlRepository,
    ITimeService timeService,
    ApplicationReviewMapper applicationReviewMapper,
    LegacyApplicationMapper legacyApplicationMapper)
{
    private const int BatchSize = 50;
    private const int MaxRuntimeInSeconds = 270; // 4m 30s

    public async Task RunAsync(List<Guid>? ids = null)
    {
        var startTime = timeService.UtcNow;
        
        var applicationReviews = ids is { Count: > 0 }
            ? await mongoRepository.FetchBatchByIdsAsync(ids)
            : await mongoRepository.FetchBatchAsync(BatchSize);
        
        while (applicationReviews is { Count: > 0 } && timeService.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            logger.LogInformation("Started processing a batch of {count} records", applicationReviews.Count);
            
            // Create the legacy applications
            var legacyApplications = applicationReviews
                .Where(x => x.Application is not null && x.Application?.IsFaaV2Application is not true)
                .Select(x => legacyApplicationMapper.MapFrom(x.Id, x.Application!))
                .ToList();

            if (legacyApplications is { Count: >0 })
            {
                await sqlRepository.UpsertLegacyApplicationsBatchAsync(legacyApplications);
                logger.LogInformation("Imported {count} legacy applications", legacyApplications.Count);
            }
            
            // Fetch the associated vacancies
            var vacancyReferences = applicationReviews.Select(x => x.VacancyReference).Distinct().ToList();
            var vacancies = await mongoRepository.FetchVacanciesAsync(vacancyReferences);
            
            // Map the records 
            var mappedRecords = applicationReviews.Select(x => applicationReviewMapper.MapFrom(x, vacancies)).ToList();
            
            // Upsert into SQL
            if (mappedRecords is { Count: > 0 })
            {
                await sqlRepository.UpsertApplicationReviewsBatchAsync(mappedRecords);
                logger.LogInformation("Imported {count} application reviews", mappedRecords.Count);
            }
            
            // Mark migrated
            await mongoRepository.UpdateMigratedDateBatchAsync(applicationReviews.Select(x => x.Id).ToList());
            logger.LogInformation("Marked application reviews as migrated");
            
            // Fetch the next batch
            applicationReviews = await mongoRepository.FetchBatchAsync(BatchSize);
        }
    }
}