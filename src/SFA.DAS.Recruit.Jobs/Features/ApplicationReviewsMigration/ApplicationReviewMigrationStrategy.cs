using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core.Services;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

public class ApplicationReviewMigrationStrategy(
    ILogger<ApplicationReviewMigrationStrategy> logger,
    ApplicationReviewsMigrationRepository repository,
    ITimeService timeService)
{
    private const int BatchSize = 1;

    public async Task RunAsync()
    {        
        var applicationReviews = await repository.FetchBatch(BatchSize);
        while (applicationReviews is { Count: > 0 } && timeService.GmtNow is { Hour: <5 })
        {
            var migratedIds = await MigrateBatch(applicationReviews);
            await repository.MarkMigrated(migratedIds);
            applicationReviews = await repository.FetchBatch(BatchSize);
        }
    }

    private async Task<List<Guid>> MigrateBatch(IEnumerable<ApplicationReview> applicationReviews)
    {
        var migratedIds = new List<Guid>();
        foreach (var applicationReview in applicationReviews)
        {
            // repository.Upsert(applicationReview);
            migratedIds.Add(applicationReview.Id);
        }
        
        return migratedIds;
    }
}