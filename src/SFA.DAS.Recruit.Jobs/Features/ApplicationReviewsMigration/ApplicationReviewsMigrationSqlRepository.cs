using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

[ExcludeFromCodeCoverage]
public class ApplicationReviewsMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertApplicationReviewsBatchAsync(List<ApplicationReview> applicationReviews)
    {
        var strategy = dataContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteInTransactionAsync(async token =>
        {
            await dataContext.BulkInsertOrUpdateAsync(
                applicationReviews,
                new BulkConfig { UseTempDB = true },
                cancellationToken: token);
        },
        _ => Task.FromResult(true));
    }

    public async Task UpsertLegacyApplicationsBatchAsync(List<LegacyApplication> legacyApplications)
    {
        await dataContext.BulkInsertOrUpdateAsync(legacyApplications);
    }
}