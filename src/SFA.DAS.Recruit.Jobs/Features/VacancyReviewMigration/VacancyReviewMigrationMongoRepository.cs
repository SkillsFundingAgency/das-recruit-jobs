using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyReviewMigration;

[ExcludeFromCodeCoverage]
public class VacancyReviewMigrationMongoRepository(
    ILoggerFactory loggerFactory,
    IOptions<MongoDbConnectionDetails> config)
    : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, config)
{
    private static readonly DateTime RemigrationCutOff = new(2025, 5, 30); // cause everything to get migrated again
    public async Task<List<VacancyReview>> FetchBatchAsync(int batchSize, int maxAgeInDays)
    {
        var createdAfterDate = DateTime.UtcNow.AddDays(-maxAgeInDays);
        var collection = GetCollection<VacancyReview>(MongoDbCollectionNames.VacancyReviews);
        var pipeline = new EmptyPipelineDefinition<VacancyReview>()
            .Match(x => (x.MigrationDate == null || x.MigrationDate < RemigrationCutOff) && x.MigrationFailed == null && x.MigrationIgnore == null && x.CreatedDate > createdAfterDate)
            .Limit(batchSize);

        return await RetryPolicy.ExecuteAsync(
            async _ => await (await collection.AggregateAsync(pipeline)).ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }
    
    public async Task UpdateSuccessMigrationDateBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<VacancyReview>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<VacancyReview>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, null);
        var collection = GetCollection<VacancyReview>(MongoDbCollectionNames.VacancyReviews);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateSuccessMigrationDateBatchAsync))
        );
    }
    
    public async Task UpdateFailedMigrationDateBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<VacancyReview>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<VacancyReview>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, true);
        var collection = GetCollection<VacancyReview>(MongoDbCollectionNames.VacancyReviews);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateFailedMigrationDateBatchAsync))
        );
    }
    
    public async Task UpdateIgnoreMigrationBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<VacancyReview>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<VacancyReview>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationIgnore, true);
        var collection = GetCollection<VacancyReview>(MongoDbCollectionNames.VacancyReviews);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateFailedMigrationDateBatchAsync))
        );
    }
}