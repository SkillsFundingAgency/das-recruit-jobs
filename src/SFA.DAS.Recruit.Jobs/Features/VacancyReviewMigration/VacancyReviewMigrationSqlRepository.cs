using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyReviewMigration;

[ExcludeFromCodeCoverage]
public class VacancyReviewMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertVacancyReviewBatchAsync(List<VacancyReview> vacancyReviews)
    {
        await dataContext.BulkInsertOrUpdateAsync(vacancyReviews);
    }
}