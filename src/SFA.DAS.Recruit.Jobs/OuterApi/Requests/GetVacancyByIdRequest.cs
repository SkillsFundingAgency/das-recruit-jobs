using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public readonly struct GetVacancyByIdRequest(Guid id) : IGetRequest
{
    public string Url => $"vacancies/{id}";
}