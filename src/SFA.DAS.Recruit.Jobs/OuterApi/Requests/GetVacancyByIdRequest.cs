using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public class GetVacancyByIdRequest(Guid id) : IGetRequest
{
    public string Url => $"vacancies/{id}";
}