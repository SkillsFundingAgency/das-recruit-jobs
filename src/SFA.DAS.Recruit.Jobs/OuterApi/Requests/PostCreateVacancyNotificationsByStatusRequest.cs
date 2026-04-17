using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Domain;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public class PostCreateVacancyNotificationsByStatusRequest(Guid vacancyId, VacancyStatus status): IPostRequest
{
    public string Url => $"notifications/{status}/create/vacancies/{vacancyId}";
    public object? Data => null;
}