using EFCore.BulkExtensions;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyAnalyticsMigration;
[ExcludeFromCodeCoverage]
public class VacancyAnalyticsSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertVacancyAnalyticsBatchAsync(List<VacancyAnalytics> vacancyAnalytics)
    {
        await dataContext.BulkInsertOrUpdateAsync(vacancyAnalytics);
    }
}