using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public sealed class GetProviderOwnedVacanciesInReviewRequest(long ukprn, long legalEntityId) : IGetRequest
{
    public string Url => "vacancies/transferable/in-review".WithQueryParams(("ukprn", $"{ukprn}"), ("legalEntityId", $"{legalEntityId}"));
}