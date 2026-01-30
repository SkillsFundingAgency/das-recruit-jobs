using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

[ExcludeFromCodeCoverage]
public class VacancyMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertVacanciesBatchAsync(List<Vacancy> vacancies)
    {
        await dataContext.BulkInsertOrUpdateAsync(vacancies);
    }

    public async Task UpdateVacancyWageInfo(Guid id, decimal? weeklyHours, decimal? fixedWageYearlyAmount)
    {
        await dataContext
            .Vacancy
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(v => v.Wage_WeeklyHours, weeklyHours)
                    .SetProperty(v => v.Wage_FixedWageYearlyAmount, fixedWageYearlyAmount)
            );
    }
}