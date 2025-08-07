using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

[ExcludeFromCodeCoverage]
public class ApplicationReviewsMigrationMongoRepository(
    ILoggerFactory loggerFactory,
    IOptions<MongoDbConnectionDetails> config, IMongoClient mongoClient)
    : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, config, mongoClient)
{
    public async Task<List<ApplicationReview>> FetchBatchAsync(int batchSize)
    {
        var remigrateIfBeforeDate = new DateTime(2025, 08, 07); // set to a date after a migration to trigger reimport
        var migrateIfCreatedAfterDate = new DateTime(2025, 05, 01);
        
        var collection = GetCollection<ApplicationReview>(MongoDbCollectionNames.ApplicationReviews);
        return await RetryPolicy.ExecuteAsync(
            _ => collection
                .Find(x=> 
                    x.MigrationDate < remigrateIfBeforeDate &&
                    x.CreatedDate > migrateIfCreatedAfterDate,
                    new FindOptions{MaxTime = TimeSpan.FromMinutes(10), BatchSize = batchSize})
                .Limit(batchSize)
                .ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }

    public async Task<List<ApplicationReview>> FetchBatchByIdsAsync(List<Guid> ids)
    {
        var collection = GetCollection<ApplicationReview>(MongoDbCollectionNames.ApplicationReviews);
        var filterDefinition = Builders<ApplicationReview>.Filter.In(x => x.Id, ids);

        return await RetryPolicy.ExecuteAsync(
            _ => collection.Find(filterDefinition, new FindOptions{MaxTime = TimeSpan.FromMinutes(10)}).ToListAsync(),
            new Context(nameof(FetchBatchByIdsAsync))
        );
    }
    
    public async Task<List<ApplicationReview>> FetchBatchByVacancyReferenceAsync(long vacancyReference)
    {
        var collection = GetCollection<ApplicationReview>(MongoDbCollectionNames.ApplicationReviews);
        return await RetryPolicy.ExecuteAsync(
            _ => collection
                .Find(x => x.VacancyReference == vacancyReference, new FindOptions { MaxTime = TimeSpan.FromMinutes(10) })
                .ToListAsync(),
            new Context(nameof(FetchBatchByVacancyReferenceAsync))
        );
    }

    public async Task UpdateSuccessMigrationDateBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<ApplicationReview>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<ApplicationReview>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, null);
        var collection = GetCollection<ApplicationReview>(MongoDbCollectionNames.ApplicationReviews);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateSuccessMigrationDateBatchAsync))
        );
    }
    
    public async Task UpdateFailedMigrationDateBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<ApplicationReview>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<ApplicationReview>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, true);
        var collection = GetCollection<ApplicationReview>(MongoDbCollectionNames.ApplicationReviews);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateFailedMigrationDateBatchAsync))
        );
    }

    public async Task<List<Vacancy>> FetchVacanciesAsync(List<long> vacancyReferences)
    {
        var filterDef = Builders<Vacancy>.Filter.In(x => x.VacancyReference!.Value, vacancyReferences);
        var collection = GetCollection<Vacancy>(MongoDbCollectionNames.Vacancies);
        
        return await RetryPolicy.ExecuteAsync(_ => collection
                .Find(filterDef, new FindOptions{MaxTime = TimeSpan.FromMinutes(10)})
                .Project<Vacancy>(GetProjection<Vacancy>())
                .ToListAsync(),
            new Context(nameof(FetchVacanciesAsync))
        );
    }
}