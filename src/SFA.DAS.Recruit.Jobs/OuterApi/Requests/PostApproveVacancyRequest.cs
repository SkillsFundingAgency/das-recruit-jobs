using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public class PostApproveVacancyRequest(Guid vacancyId): IPostRequest
{
    public string Url => $"vacancies/{vacancyId}/approve";
    public object? Data => null;
}