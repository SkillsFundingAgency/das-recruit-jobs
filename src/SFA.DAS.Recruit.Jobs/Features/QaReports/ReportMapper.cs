using System.Text.Json;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.QaReports;

public class ReportMapper
{
    public Report MapFrom(DataAccess.MongoDb.Domain.Report source)
    {
        return new Report
        {
            CreatedBy = source.RequestedBy.Email,
            CreatedDate = source.RequestedOn,
            DownloadCount = source.DownloadCount,
            DynamicCriteria = JsonSerializer.Serialize(source.Parameters, RecruitJobsDataContext.JsonOptions),
            Id = source.Id,
            OwnerType = ReportOwnerType.Qa,
            Type = ReportType.QaApplications,
            Name = source.ReportName,
            UserId = source.RequestedBy.Email
        };
    }
}