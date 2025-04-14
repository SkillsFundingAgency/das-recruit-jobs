using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.EmployerProfilesMigration;

[ExcludeFromCodeCoverage]
public class EmployerProfilesMigrationMongoRepository(
    ILoggerFactory loggerFactory,
    IOptions<MongoDbConnectionDetails> config)
    : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, config)
{
    public async Task<List<EmployerProfile>> FetchBatchAsync(int batchSize)
    {
        var collection = GetCollection<EmployerProfile>(MongoDbCollectionNames.EmployerProfiles);
        var pipeline = new EmptyPipelineDefinition<EmployerProfile>()
            .Match(x => x.MigrationFailed != true && (x.MigrationDate == null || x.MigrationDate < x.LastUpdatedDate))
            .Limit(batchSize);

        return await RetryPolicy.ExecuteAsync(
            async _ => await (await collection.AggregateAsync(pipeline)).ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }
    
    // public async Task<List<EmployerProfile>> FetchBatchByIdsAsync(List<string> ids)
    // {
    //     var collection = GetCollection<EmployerProfile>(MongoDbCollectionNames.EmployerProfiles);
    //     var filterDefinition = Builders<EmployerProfile>.Filter.In(x => x.Id, ids);
    //
    //     return await RetryPolicy.ExecuteAsync(
    //         _ => collection.Find(filterDefinition).ToListAsync(),
    //         new Context(nameof(FetchBatchAsync))
    //     );
    // }

    public async Task UpdateSuccessMigrationDateBatchAsync(List<string> ids)
    {
        var filterDef = Builders<EmployerProfile>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<EmployerProfile>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow);
        var collection = GetCollection<EmployerProfile>(MongoDbCollectionNames.EmployerProfiles);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateSuccessMigrationDateBatchAsync))
        );
    }

    public async Task UpdateFailedMigrationDateBatchAsync(List<string> ids)
    {
        var filterDef = Builders<EmployerProfile>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<EmployerProfile>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, true);
        var collection = GetCollection<EmployerProfile>(MongoDbCollectionNames.EmployerProfiles);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateFailedMigrationDateBatchAsync))
        );
    }
}