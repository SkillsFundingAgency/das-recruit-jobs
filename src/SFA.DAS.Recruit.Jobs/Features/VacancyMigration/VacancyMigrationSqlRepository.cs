using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
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
}