using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.BlockedOrganisationsMigration;

[ExcludeFromCodeCoverage]
public class BlockedOrganisationMigrationMongoRepository(
    ILoggerFactory loggerFactory,
    IOptions<MongoDbConnectionDetails> config, IMongoClient mongoClient)
    : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, config, mongoClient)
{
    public async Task<List<BlockedOrganisation>> FetchBatchAsync(int batchSize, DateTime remigrateIfBeforeDate)
    {
        var collection = GetCollection<BlockedOrganisation>(MongoDbCollectionNames.BlockedOrganisations);
        
        return await RetryPolicy.ExecuteAsync(
            _ => collection
                .Find(x => (x.MigrationDate == null || x.MigrationDate < remigrateIfBeforeDate), new FindOptions{ MaxTime = TimeSpan.FromMinutes(5), BatchSize = batchSize })
                .Limit(batchSize)
                .Project<BlockedOrganisation>(GetProjection<BlockedOrganisation>())
                .ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }

    public async Task UpdateSuccessMigrationDateBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<BlockedOrganisation>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<BlockedOrganisation>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, null);
        var collection = GetCollection<BlockedOrganisation>(MongoDbCollectionNames.BlockedOrganisations);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateSuccessMigrationDateBatchAsync))
        );
    }
}