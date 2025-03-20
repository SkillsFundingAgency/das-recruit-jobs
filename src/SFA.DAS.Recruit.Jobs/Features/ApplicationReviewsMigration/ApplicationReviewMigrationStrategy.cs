using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core.Services;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

public class ApplicationReviewMigrationStrategy(
    ILogger<ApplicationReviewMigrationStrategy> logger,
    ApplicationReviewsMigrationMongoRepository mongoRepository,
    ApplicationReviewsMigrationSqlRepository sqlRepository,
    ITimeService timeService,
    ApplicationReviewMapper mapper)
{
    private const int BatchSize = 50;

    public async Task RunAsync()
    {
        var applicationReviews = await mongoRepository.FetchBatchAsync(BatchSize);
        while (applicationReviews is { Count: > 0 } && timeService.GmtNow is { Hour: <500 })
        {
            logger.LogInformation("Processing {count} records", applicationReviews.Count);
            
            // Fetch the associated vacancies
            var vacancyReferences = applicationReviews.Select(x => x.VacancyReference).Distinct();
            var vacancies = await mongoRepository.FetchVacanciesAsync(vacancyReferences);
            
            // Map the records 
            var mappedRecords = applicationReviews.Select(x => mapper.MapFrom(x, vacancies));
            
            // Upsert into SQL
            await sqlRepository.UpsertBatchAsync(mappedRecords);
            
            // Mark migrated
            await mongoRepository.BulkSetMigratedAsync(applicationReviews.Select(x => x.Id));
            
            // Fetch the next batch
            applicationReviews = await mongoRepository.FetchBatchAsync(BatchSize);
        }
    }
}