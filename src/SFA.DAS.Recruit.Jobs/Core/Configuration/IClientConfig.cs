namespace SFA.DAS.Recruit.Jobs.Core.Configuration;

public interface IClientConfig
{
    string? BaseUrl { get; set; }
    string? Key { get; set; }
}