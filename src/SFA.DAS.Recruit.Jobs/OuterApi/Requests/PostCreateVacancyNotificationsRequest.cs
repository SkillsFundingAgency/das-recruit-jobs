using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public class PostCreateVacancyNotificationsRequest(Guid vacancyId): IPostRequest
{
    public string Url => $"notifications/create/vacancies/{vacancyId}";
    public object? Data => null;
}