using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyAnalyticsMigration;
[ExcludeFromCodeCoverage]
public class VacancyAnalyticsSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertVacancyAnalyticsBatchAsync(List<VacancyAnalytics> vacancyAnalytics, CancellationToken cancellationToken)
    {
        foreach (var vacancyAnalytic in vacancyAnalytics)
        {
            var existing = await FindVacancyAnalyticsByVacancyReference(vacancyAnalytic.VacancyReference);

            if (existing is null)
            {
                // Insert new
                dataContext.VacancyAnalytics.Add(vacancyAnalytic);
            }
            else
            {
                // Update existing
                existing.UpdatedDate = vacancyAnalytic.UpdatedDate;
                existing.Analytics = vacancyAnalytic.Analytics;
            }
        }

        await dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<VacancyAnalytics?> FindVacancyAnalyticsByVacancyReference(long vacancyReference)
    {
        return await dataContext.VacancyAnalytics.FindAsync(vacancyReference);
    }
}