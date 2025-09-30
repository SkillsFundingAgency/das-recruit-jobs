using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Core.Configuration;

[ExcludeFromCodeCoverage]
public class RecruitJobsConfiguration
{
    public string? QueueStorage { get; set; }
}