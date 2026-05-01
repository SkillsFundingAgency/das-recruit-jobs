using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive;

public class VacancyArchivingSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertVacanciesBatchAsync(List<Vacancy> vacancies)
    {
        await dataContext.BulkInsertOrUpdateAsync(vacancies);
    }

    public async Task<List<Vacancy>> GetClosedVacancies(DateTime pointInTime, int batchSize, CancellationToken cancellationToken)
    {
        return await dataContext
            .Vacancy
            .Where(x => 
                x.ClosingDate != null
                && x.DeletedDate == null
                && x.Status == VacancyStatus.Closed
                && x.ClosedDate <= pointInTime)
            .Take(batchSize)
            .ToListAsync(cancellationToken: cancellationToken);
    }
}