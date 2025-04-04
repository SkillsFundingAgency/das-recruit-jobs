using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql;

[ExcludeFromCodeCoverage]
public class RecruitJobsConfiguration
{
    public string ConnectionString { get; set; }
}