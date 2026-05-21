using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public class PostPublishVacancyRequest(Guid vacancyId): IPostRequest
{
    public string Url => $"vacancies/{vacancyId}/publish";
    public object? Data => null;
}