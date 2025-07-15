using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.UserMigration;

[ExcludeFromCodeCoverage]
public class UserMigrationMongoRepository(
    ILoggerFactory loggerFactory,
    IOptions<MongoDbConnectionDetails> config, IMongoClient mongoClient)
    : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, config, mongoClient)
{
    public async Task<List<User>> FetchBatchAsync(int batchSize)
    {
        var collection = GetCollection<User>(MongoDbCollectionNames.Users);
        
        return await RetryPolicy.ExecuteAsync(
            _ => collection
                .Find(x => x.MigrationDate == null && !string.IsNullOrEmpty(x.Email), new FindOptions{ MaxTime = TimeSpan.FromMinutes(5), BatchSize = batchSize })
                .Limit(batchSize)
                .Project<User>(GetProjection<User>())
                .ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }

    public async Task<List<User>> FetchBatchByIdsAsync(List<Guid> ids)
    {
        var collection = GetCollection<User>(MongoDbCollectionNames.Users);
        var filterDefinition = Builders<User>.Filter.In(x => x.Id, ids);

        return await RetryPolicy.ExecuteAsync(
            _ => collection
                .Find(filterDefinition, new FindOptions{ MaxTime = TimeSpan.FromMinutes(1) })
                .Project<User>(GetProjection<User>())
                .ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }

    public async Task UpdateSuccessMigrationDateBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<User>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<User>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, null);
        var collection = GetCollection<User>(MongoDbCollectionNames.Users);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateSuccessMigrationDateBatchAsync))
        );
    }
    
    public async Task UpdateFailedMigrationDateBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<User>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<User>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, true);
        var collection = GetCollection<User>(MongoDbCollectionNames.Users);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateFailedMigrationDateBatchAsync))
        );
    }
}