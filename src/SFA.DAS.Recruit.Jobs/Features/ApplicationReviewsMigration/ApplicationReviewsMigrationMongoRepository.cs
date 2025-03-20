using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using SFA.DAS.Recruit.Jobs.Core.Services;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

public class ApplicationReviewsMigrationMongoRepository(
    ILoggerFactory loggerFactory,
    IOptions<MongoDbConnectionDetails> config,
    ITimeService timeService)
    : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, config)
{
    public async Task<List<ApplicationReview>> FetchBatchAsync(int batchSize)
    {
        var collection = GetCollection<ApplicationReview>(MongoDbCollectionNames.ApplicationReviews);
        var pipeline = new EmptyPipelineDefinition<ApplicationReview>()
            .Match(x => x.MigratedDate == null || x.MigratedDate < x.StatusUpdatedDate)
            .Limit(batchSize);

        return await RetryPolicy.ExecuteAsync(
            async _ => await (await collection.AggregateAsync(pipeline)).ToListAsync(),
            new Context(nameof(FetchBatchAsync))
        );
    }

    public async Task BulkSetMigratedAsync(IEnumerable<Guid> ids)
    {
        var filterDef = Builders<ApplicationReview>.Filter.In(x => x.Id, ids);
        var updateDef = Builders<ApplicationReview>.Update.Set(x => x.MigratedDate, timeService.UtcNow);
        var collection = GetCollection<ApplicationReview>(MongoDbCollectionNames.ApplicationReviews);
        await RetryPolicy.ExecuteAsync(
            _ => collection.UpdateManyAsync(filterDef, updateDef),
            new Context(nameof(BulkSetMigratedAsync))
        );
    }

    public async Task<List<Vacancy>> FetchVacanciesAsync(IEnumerable<long> vacancyReferences)
    {
        var filterDef = Builders<Vacancy>.Filter.In(x => x.VacancyReference, vacancyReferences);
        var collection = GetCollection<Vacancy>(MongoDbCollectionNames.Vacancies);
        
        return await RetryPolicy.ExecuteAsync(_ => collection
                .Find(filterDef)
                .Project<Vacancy>(GetProjection<Vacancy>())
                .ToListAsync(),
            new Context(nameof(FetchVacanciesAsync))
        );
    }
}