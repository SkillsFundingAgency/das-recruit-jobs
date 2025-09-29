using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Core.Infrastructure;

[ExcludeFromCodeCoverage]
public class RecruitJobsConfiguration
{
    public string ApimBaseUrl { get; set; }
    public string ApimKey { get; set; }
    public string QueueStorage { get; set; }
}