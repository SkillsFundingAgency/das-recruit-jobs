using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using MongoVacancy = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.Vacancy;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

[ExcludeFromCodeCoverage]
public class VacancyWageMigrationStrategy(
    ILogger<VacancyWageMigrationStrategy> logger,
    VacancyMigrationMongoRepository mongoRepository,
    VacancyMigrationSqlRepository sqlRepository)
{
    private const int BatchSize = 200;
    private const int MaxRuntimeInSeconds = 270; // 4m 30s
    
    public async Task RunAsync(List<Guid> ids)
    {
        var mongoVacancies = await mongoRepository.FetchBatchByIdsAsync(ids);
        await ProcessBatchAsync(mongoVacancies);
    }
    
    public async Task RunAsync()
    {
        var startTime = DateTime.UtcNow;
        var remigrateIfBeforeDate = new DateTime(2026, 01, 21); // set to a date after a migration to trigger reimport
        var mongoVacancies = await mongoRepository.FetchBatchAsync(BatchSize, remigrateIfBeforeDate);
        while (mongoVacancies is { Count: > 0 } && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            await ProcessBatchAsync(mongoVacancies);
            mongoVacancies = await mongoRepository.FetchBatchAsync(BatchSize, remigrateIfBeforeDate);
        }
    }

    private async Task ProcessBatchAsync(List<MongoVacancy> vacancies)
    {
        List<MongoVacancy> excluded = [];
        List<MongoVacancy> processed = [];
        
        foreach (var vacancy in vacancies)
        {
            if (vacancy.IsDeleted)
            {
                processed.Add(vacancy);
                continue;
            }

            try
            {
                await sqlRepository.UpdateVacancyWageInfo(vacancy.Id, vacancy.Wage?.WeeklyHours, vacancy.Wage?.FixedWageYearlyAmount);
                processed.Add(vacancy);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to update vacancy wage info");
                excluded.Add(vacancy);
            }
        }
        
        if (excluded is { Count: > 0 })
        {
            await mongoRepository.UpdateFailedMigrationDateBatchAsync(excluded.Select(x => x.Id).ToList());
            logger.LogInformation("Failed to migrate {FailedCount} Vacancies", excluded.Count);
        }

        if (processed is { Count: > 0 })
        {
            await mongoRepository.UpdateSuccessMigrationDateBatchAsync(processed.Select(x => x.Id).ToList());
            logger.LogInformation("Marked {SuccessCount} Vacancies as migrated", processed.Count);
        }
    }
}