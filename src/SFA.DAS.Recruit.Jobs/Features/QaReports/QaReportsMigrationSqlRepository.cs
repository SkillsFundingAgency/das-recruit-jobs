using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.QaReports;

[ExcludeFromCodeCoverage]
public class QaReportsMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertReportsBatchAsync(List<Report> reports)
    {
        foreach (var report in reports)
        {
            var existingReport = await dataContext.Report.FirstOrDefaultAsync(x => x.Id == report.Id);

            if (existingReport == null)
            {
                dataContext.Report.Add(report);
            }
            else
            {
                dataContext.Entry(existingReport).CurrentValues.SetValues(report);
            }
        }

        await dataContext.SaveChangesAsync();
    }
}
