using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyAnalyticsMigration;
[ExcludeFromCodeCoverage]
public class VacancyAnalyticsMigrationMongoRepository(ILoggerFactory loggerFactory,
IOptions<MongoDbConnectionDetails> config)
: MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, config)
{
    public async Task<List<VacancyAnalyticsSummaryV2>> GetAllVacancyAnalyticsSummariesAsync(int batchSize,
        DateTime cutOffDateTime,
        CancellationToken cancellationToken = default)
    {
        var collection = GetCollection<VacancyAnalyticsSummaryV2>(MongoDbCollectionNames.QueryStore);

        Logger.LogInformation("Fetching ALL VacancyAnalyticsSummaryV2 documents…");

        var pipeline = new EmptyPipelineDefinition<VacancyAnalyticsSummaryV2>()
            .Match(x => x.MigrationFailed != true && (x.MigrationDate == null || x.MigrationDate < x.LastUpdated))
            .Match(x => x.ViewType == "VacancyAnalyticsSummaryV2" && x.LastUpdated < cutOffDateTime && x.VacancyReference > 0)
            .Limit(batchSize);

        var result = await RetryPolicy.ExecuteAsync(
            async _ => await (await collection.AggregateAsync(pipeline, cancellationToken: cancellationToken)).ToListAsync(cancellationToken: cancellationToken),
            new Context(nameof(GetAllVacancyAnalyticsSummariesAsync))
        );

        Logger.LogInformation("Fetched {Count} VacancyAnalyticsSummary records.", result.Count);

        return result;
    }

    public async Task UpdateSuccessMigrationDateBatchAsync(List<string> ids)
    {
        var filterDef = Builders<VacancyAnalyticsSummaryV2>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<VacancyAnalyticsSummaryV2>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow);
        var collection = GetCollection<VacancyAnalyticsSummaryV2>(MongoDbCollectionNames.QueryStore);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateSuccessMigrationDateBatchAsync))
        );
    }

    public async Task UpdateFailedMigrationDateBatchAsync(List<string> ids)
    {
        var filterDef = Builders<VacancyAnalyticsSummaryV2>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<VacancyAnalyticsSummaryV2>.Update
            .Set(x => x.MigrationDate, DateTime.UtcNow)
            .Set(x => x.MigrationFailed, true);
        var collection = GetCollection<VacancyAnalyticsSummaryV2>(MongoDbCollectionNames.QueryStore);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(UpdateFailedMigrationDateBatchAsync))
        );
    }
}