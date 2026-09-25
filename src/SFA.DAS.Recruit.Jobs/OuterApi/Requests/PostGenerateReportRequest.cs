using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public class PostGenerateReportRequest(Guid reportId) : IPostRequest
{
    public string Url => $"reports/generate/{reportId}";
    public object? Data => null;
}