using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using MongoVacancy = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.Vacancy;
using SqlVacancy = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.Vacancy;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

[ExcludeFromCodeCoverage]
public class VacancyMigrationStrategy(
    ILogger<VacancyMigrationStrategy> logger,
    VacancyMapper mapper,
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
        var mongoVacancies = await mongoRepository.FetchBatchAsync(BatchSize);
        while (mongoVacancies is { Count: > 0 } && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            await ProcessBatchAsync(mongoVacancies);
            mongoVacancies = await mongoRepository.FetchBatchAsync(BatchSize);
        }
    }

    private async Task ProcessBatchAsync(List<MongoVacancy> vacancies)
    {
        List<MongoVacancy> excluded = [];
        List<SqlVacancy> mappedVacancies = [];
        foreach (var vacancy in vacancies)
        {
            var item = await mapper.MapFromAsync(vacancy);
            if (item == SqlVacancy.None)
            {
                excluded.Add(vacancy);
            }
            else
            {
                mappedVacancies.Add(item);                
            }
        }
        
        if (excluded is { Count: > 0 })
        {
            await mongoRepository.UpdateFailedMigrationDateBatchAsync(excluded.Select(x => x.Id).ToList());
            logger.LogInformation("Failed to migrate {FailedCount} vacancy reviews", excluded.Count);
        }

        if (mappedVacancies is { Count: > 0 })
        {
            await sqlRepository.UpsertVacanciesBatchAsync(mappedVacancies);
            logger.LogInformation("Imported {count} vacancy reviews", mappedVacancies.Count);
                
            await mongoRepository.UpdateSuccessMigrationDateBatchAsync(mappedVacancies.Select(x => x.Id).ToList());
            logger.LogInformation("Marked {SuccessCount} vacancy reviews as migrated", mappedVacancies.Count);
        }
    }
}