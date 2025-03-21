using System.Collections.Generic;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

public class ApplicationReviewsMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertApplicationReviewsBatchAsync(List<ApplicationReview> applicationReviews)
    {
        await dataContext.BulkInsertOrUpdateAsync(applicationReviews);
    }

    public async Task UpsertLegacyApplicationsBatchAsync(List<LegacyApplication> legacyApplications)
    {
        await dataContext.BulkInsertOrUpdateAsync(legacyApplications);
    }
}