using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.QaReports;

[ExcludeFromCodeCoverage]
public class QaReportsMigrationMongoRepository(
    ILoggerFactory loggerFactory,
    IOptions<MongoDbConnectionDetails> config, IMongoClient mongoClient)
    : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, config, mongoClient)
{
    private const string QaApplicationsReportType = "QaApplications";

    public async Task<List<Report>> FetchBatchAsync(int batchSize, DateTime remigrateIfBeforeDate)
    {
        var collection = GetCollection<Report>(MongoDbCollectionNames.Reports);

        return await RetryPolicy.ExecuteAsync(
            _ => collection
                .Find(x => x.RerportType == QaApplicationsReportType && (x.MigrationDate == null || x.MigrationDate < remigrateIfBeforeDate), new FindOptions { MaxTime = TimeSpan.FromMinutes(5), BatchSize = batchSize })
                .Limit(batchSize)
                .Project<Report>(GetProjection<Report>())
                .ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }

    public async Task UpdateSuccessMigrationDateBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<Report>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<Report>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, null);
        var collection = GetCollection<Report>(MongoDbCollectionNames.Reports);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateSuccessMigrationDateBatchAsync))
        );
    }

    public async Task UpdateFailedMigrationDateBatchAsync(List<Guid> ids)
    {
        var filterDef = Builders<Report>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<Report>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, true);
        var collection = GetCollection<Report>(MongoDbCollectionNames.Reports);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateFailedMigrationDateBatchAsync))
        );
    }
}
