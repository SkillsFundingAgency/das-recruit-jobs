using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

[ExcludeFromCodeCoverage]
public class VacancyMigrationMongoRepository(
    ILoggerFactory loggerFactory,
    IOptions<MongoDbConnectionDetails> config, IMongoClient mongoClient)
    : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, config, mongoClient)
{
    public async Task<List<Vacancy>> FetchBatchAsync(int batchSize, DateTime reMigrateIfAfterDate)
    {
        var collection = GetCollection<Vacancy>(MongoDbCollectionNames.Vacancies);
        var dateTo = new DateTime(2025, 12, 9);
        
        return await RetryPolicy.ExecuteAsync(
            _ => collection
                .Find(x => (x.MigrationDate <= dateTo) 
                           && x.VacancyType != VacancyType.Traineeship 
                           && (x.ApprenticeshipType == ApprenticeshipTypes.Standard || x.ApprenticeshipType == ApprenticeshipTypes.Foundation)
                           && x.Status == VacancyStatus.Closed, new FindOptions{ MaxTime = TimeSpan.FromMinutes(5), BatchSize = batchSize })
                .Limit(batchSize)
                .Project<Vacancy>(GetProjection<Vacancy>())
                .ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }

    public async Task<List<Vacancy>> FetchBatchByIdsAsync(List<Guid> ids)
    {
        var collection = GetCollection<Vacancy>(MongoDbCollectionNames.Vacancies);
        var filterDefinition = Builders<Vacancy>.Filter.In(x => x.Id, ids);

        return await RetryPolicy.ExecuteAsync(
            _ => collection
                .Find(filterDefinition, new FindOptions{ MaxTime = TimeSpan.FromMinutes(1) })
                .Project<Vacancy>(GetProjection<Vacancy>())
                .ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }

    public async Task UpdateSuccessMigrationDateBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<Vacancy>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<Vacancy>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, null);
        var collection = GetCollection<Vacancy>(MongoDbCollectionNames.Vacancies);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateSuccessMigrationDateBatchAsync))
        );
    }
    
    public async Task UpdateFailedMigrationDateBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<Vacancy>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<Vacancy>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, true);
        var collection = GetCollection<Vacancy>(MongoDbCollectionNames.Vacancies);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateFailedMigrationDateBatchAsync))
        );
    }
}