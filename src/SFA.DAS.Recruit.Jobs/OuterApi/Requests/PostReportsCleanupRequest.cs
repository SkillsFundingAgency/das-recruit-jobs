using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public class PostReportsCleanupRequest : IPostRequest
{
    public string Url => "reports/cleanup";
    public object? Data => null;
}