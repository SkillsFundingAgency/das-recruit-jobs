using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Core.Configuration;

[ExcludeFromCodeCoverage]
public class RecruitJobsOuterApiConfiguration
{
    public string? BaseUrl { get; set; }
    public string? Key { get; set; }
}