using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;

public interface IRecruitAiOuterClient
{
    Task<ApiResponse> SubmitVacancyForAiReviewAsync(Guid vacancyId, Guid vacancyReviewId, CancellationToken cancellationToken);
}

/// <summary>
/// Encapsulates the AI functions on Recruit Jobs Outer Api
/// </summary>
public class RecruitAiOuterClient(HttpClient httpClient, RecruitJobsOuterApiConfiguration config, JsonSerializerOptions jsonSerializationOptions)
    : ClientBase<RecruitJobsOuterApiConfiguration>(httpClient, config, jsonSerializationOptions), IRecruitAiOuterClient
{
    public async Task<ApiResponse> SubmitVacancyForAiReviewAsync(Guid vacancyId, Guid vacancyReviewId, CancellationToken cancellationToken)
    {
        return await PostAsync<NoResponse>($"ai/vacancy/{vacancyId}/review", new { vacancyReviewId }, cancellationToken: cancellationToken);
    }
}