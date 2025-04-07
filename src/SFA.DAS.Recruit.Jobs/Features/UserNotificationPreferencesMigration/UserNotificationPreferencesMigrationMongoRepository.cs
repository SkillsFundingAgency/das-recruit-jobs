using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;

[ExcludeFromCodeCoverage]
public class UserNotificationPreferencesMigrationMongoRepository(
    ILoggerFactory loggerFactory,
    IOptions<MongoDbConnectionDetails> config)
    : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, config)
{
    public async Task<List<UserNotificationPreferences>> FetchBatchAsync(int batchSize)
    {
        var remigrateAfter = DateTime.UtcNow.AddDays(-2);
        var collection = GetCollection<UserNotificationPreferences>(MongoDbCollectionNames.UserNotificationPreferences);
        var pipeline = new EmptyPipelineDefinition<UserNotificationPreferences>()
            .Match(x => x.MigrationIgnore != true && (x.MigrationDate == null || x.MigrationDate < remigrateAfter))
            .Limit(batchSize);

        return await RetryPolicy.ExecuteAsync(
            async _ => await (await collection.AggregateAsync(pipeline)).ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }
    
    public async Task<List<UserNotificationPreferences>> FetchBatchByIdsAsync(List<string> ids)
    {
        var collection = GetCollection<UserNotificationPreferences>(MongoDbCollectionNames.UserNotificationPreferences);
        var filterDefinition = Builders<UserNotificationPreferences>.Filter.In(x => x.Id, ids);

        return await RetryPolicy.ExecuteAsync(
            _ => collection.Find(filterDefinition).ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }

    public async Task UpdateSuccessMigrationDateBatchAsync(List<string> ids)
    {
        var filterDef = Builders<UserNotificationPreferences>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<UserNotificationPreferences>.Update.Set(x => x.MigrationDate, DateTime.UtcNow);
        var collection = GetCollection<UserNotificationPreferences>(MongoDbCollectionNames.UserNotificationPreferences);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateSuccessMigrationDateBatchAsync))
        );
    }
    
    public async Task UpdateFailedMigrationDateBatchAsync(List<string> ids)
    {
        var filterDef = Builders<UserNotificationPreferences>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<UserNotificationPreferences>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationIgnore, true);
        var collection = GetCollection<UserNotificationPreferences>(MongoDbCollectionNames.UserNotificationPreferences);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateFailedMigrationDateBatchAsync))
        );
    }
}