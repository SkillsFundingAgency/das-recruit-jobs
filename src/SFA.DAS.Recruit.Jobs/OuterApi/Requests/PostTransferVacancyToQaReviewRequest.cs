using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public sealed class PostTransferVacancyToQaReviewRequest(Guid vacancyId, Guid userReference, string userEmailAddress) : IPostRequest
{
    public string Url => $"updated-employer-permissions/vacancies/{vacancyId}/transfer/in-review".WithQueryParams(("userReference", $"{userReference}"), ("userEmailAddress", $"{userEmailAddress}")); 
    public object? Data => null;
}