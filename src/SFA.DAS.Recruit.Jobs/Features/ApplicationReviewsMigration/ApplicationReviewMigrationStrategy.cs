using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using MongoApplicationReview = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.ApplicationReview;
using SqlApplicationReview = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.ApplicationReview;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

[ExcludeFromCodeCoverage]
public class ApplicationReviewMigrationStrategy(
    ILogger<ApplicationReviewMigrationStrategy> logger,
    ApplicationReviewsMigrationMongoRepository mongoRepository,
    ApplicationReviewsMigrationSqlRepository sqlRepository,
    ApplicationReviewMapper applicationReviewMapper,
    LegacyApplicationMapper legacyApplicationMapper)
{
    private const int BatchSize = 100;
    private const int MaxRuntimeInSeconds = 270; // 4m 30s

    public async Task RunAsync(List<Guid> ids)
    {
        var applicationReviews = await mongoRepository.FetchBatchByIdsAsync(ids);
        await ProcessBatch(applicationReviews);
    }
    
    public async Task RunAsync()
    {
        var startTime = DateTime.UtcNow;
        logger.LogInformation($"Starting application review migration strategy");
        var applicationReviews = await mongoRepository.FetchBatchAsync(BatchSize);
        logger.LogInformation($"Finished getting application reviews");
        while (applicationReviews is { Count: > 0 } && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            await ProcessBatch(applicationReviews);
            applicationReviews = await mongoRepository.FetchBatchAsync(BatchSize);
        }
    }

    private async Task ProcessBatch(List<MongoApplicationReview> applicationReviews)
    {
        logger.LogInformation("Started processing a batch of {count} records", applicationReviews.Count);
        var excludedApplicationReviews = new List<MongoApplicationReview>();
        
        // Fetch the associated vacancies
        var mappedApplicationReviews = await GetApplicationReviews(applicationReviews, excludedApplicationReviews);
        if (mappedApplicationReviews is { Count: > 0 })
        {
            // Create the legacy applications
            var legacyApplications = GetLegacyApplications(applicationReviews);

            // Push the data to SQL server
            await sqlRepository.UpsertApplicationReviewsBatchAsync(mappedApplicationReviews);
            logger.LogInformation("Imported {count} application reviews", mappedApplicationReviews.Count);
            
            if (legacyApplications is { Count: >0 })
            {
                await sqlRepository.UpsertLegacyApplicationsBatchAsync(legacyApplications);
                logger.LogInformation("Imported {count} legacy applications", legacyApplications.Count);
            }
            
            // Mark migrated in Mongo
            await mongoRepository.UpdateSuccessMigrationDateBatchAsync(mappedApplicationReviews.Select(x => x.Id).ToList());
            logger.LogInformation("Marked {SuccessCount} application reviews as migrated", mappedApplicationReviews.Count);
        }

        // Mark failed migrations in Mongo
        if (excludedApplicationReviews is { Count: > 0 })
        {
            await mongoRepository.UpdateFailedMigrationDateBatchAsync(excludedApplicationReviews.Select(x => x.Id).ToList());
            logger.LogInformation("Failed to migrate {FailedCount} application reviews", excludedApplicationReviews.Count);
        }
    }

    private List<LegacyApplication> GetLegacyApplications(List<MongoApplicationReview> applicationReviews)
    {
        return applicationReviews
            .Where(x => x.Application is not null && x.Application?.IsFaaV2Application is not true)
            .Select(x => legacyApplicationMapper.MapFrom(x.Id, x.Application!))
            .ToList();
    }

    private async Task<List<SqlApplicationReview>> GetApplicationReviews(List<MongoApplicationReview> applicationReviews, List<MongoApplicationReview> excludedApplicationReviews)
    {
        // Check for missing applications
        var appReviewsWithMissingApplications = applicationReviews.Where(x => x.Application is null).ToList();
        if (appReviewsWithMissingApplications is { Count: > 0 })
        {
            applicationReviews.RemoveAll(x => appReviewsWithMissingApplications.Contains(x));
            excludedApplicationReviews.AddRange(appReviewsWithMissingApplications);
            appReviewsWithMissingApplications.ForEach(x =>
            {
                logger.LogWarning("Failed to migrate '{ApplicationReviewId}' due to missing application '{VacancyReference}'", x.Id, x.VacancyReference);
            });
        }
        
        // Get the associated vacancies
        var vacancyReferences = applicationReviews.Select(x => x.VacancyReference).Distinct().ToList();
        var vacancies = await mongoRepository.FetchVacanciesAsync(vacancyReferences);
        
        // Log missing vacancies
        var appReviewWithMissingVacancies = applicationReviews.Where(x => vacancies.FirstOrDefault(v => v.VacancyReference == x.VacancyReference) == null).ToList();
        if (appReviewWithMissingVacancies is { Count: > 0 })
        {
            applicationReviews.RemoveAll(x => appReviewWithMissingVacancies.Contains(x));
            excludedApplicationReviews.AddRange(appReviewWithMissingVacancies);
            appReviewWithMissingVacancies.ForEach(x =>
            {
                logger.LogWarning("Failed to migrate '{ApplicationReviewId}' due to missing vacancy '{VacancyReference}'", x.Id, x.VacancyReference);
            });
        }
            
        // Map the records
        var unmapped = new List<MongoApplicationReview>();
        var mappedRecords = applicationReviews
            .Select(x => {
                var item = applicationReviewMapper.MapFrom(x, vacancies);
                if (item == SqlApplicationReview.None)
                {
                    excludedApplicationReviews.Add(x);
                    unmapped.Add(x);
                }

                return item;
            })
            .Where(x => x != SqlApplicationReview.None)
            .ToList();
        
        applicationReviews.RemoveAll(x => unmapped.Contains(x));
        return mappedRecords;
    }
}