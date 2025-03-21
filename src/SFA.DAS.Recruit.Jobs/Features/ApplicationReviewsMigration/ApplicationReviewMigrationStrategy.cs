using System;
using System.Collections.Generic;
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
    ApplicationReviewMapper applicationReviewMapper,
    LegacyApplicationMapper legacyApplicationMapper)
{
    private const int BatchSize = 50;

    public async Task RunAsync(List<Guid>? ids = null)
    {
        var applicationReviews = ids is { Count: > 0 }
            ? await mongoRepository.FetchBatchByIdsAsync(ids)
            : await mongoRepository.FetchBatchAsync(BatchSize);
        
        while (applicationReviews is { Count: > 0 } && timeService.GmtNow is { Hour: <5 })
        {
            logger.LogInformation("Processing {count} records", applicationReviews.Count);
            
            // Create the legacy applications
            var legacyApplications = applicationReviews
                .Where(x => x.Application is not null && x.Application?.IsFaaV2Application is not true)
                .Select(x => legacyApplicationMapper.MapFrom(x.Id, x.Application!))
                .ToList();
            await sqlRepository.UpsertLegacyApplicationsBatchAsync(legacyApplications);
            
            // Fetch the associated vacancies
            var vacancyReferences = applicationReviews.Select(x => x.VacancyReference).Distinct().ToList();
            var vacancies = await mongoRepository.FetchVacanciesAsync(vacancyReferences);
            
            // Map the records 
            var mappedRecords = applicationReviews.Select(x => applicationReviewMapper.MapFrom(x, vacancies)).ToList();
            
            // Upsert into SQL
            await sqlRepository.UpsertApplicationReviewsBatchAsync(mappedRecords);
            
            // Mark migrated
            await mongoRepository.UpdateMigratedDateBatchAsync(applicationReviews.Select(x => x.Id).ToList());
            
            // Fetch the next batch
            applicationReviews = await mongoRepository.FetchBatchAsync(BatchSize);
        }
    }
}