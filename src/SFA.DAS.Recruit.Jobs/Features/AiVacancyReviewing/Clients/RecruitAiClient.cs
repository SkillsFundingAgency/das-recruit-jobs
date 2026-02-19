using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;

public interface IRecruitAiOuterClient
{
    Task<ApiResponse> ReviewVacancyAsync(Guid vacancyId, Guid vacancyReviewId, CancellationToken cancellationToken);
    Task<ApiResponse> CreateVacancyReviewAsync(Guid vacancyId, Guid vacancyReviewId, CancellationToken cancellationToken);
}

/// <summary>
/// Encapsulates the AI functions on Recruit Jobs Outer Api
/// </summary>
public class RecruitAiOuterClient(HttpClient httpClient, RecruitJobsOuterApiConfiguration config, JsonSerializerOptions jsonSerializationOptions)
    : ClientBase<RecruitJobsOuterApiConfiguration>(httpClient, config, jsonSerializationOptions), IRecruitAiOuterClient
{
    public async Task<ApiResponse> ReviewVacancyAsync(Guid vacancyId, Guid vacancyReviewId, CancellationToken cancellationToken)
    {
        return await PostAsync<NoResponse>($"ai/vacancies/{vacancyId}/review", vacancyReviewId, cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse> CreateVacancyReviewAsync(Guid vacancyId, Guid vacancyReviewId, CancellationToken cancellationToken)
    {
        return await PostAsync<NoResponse>($"ai/vacancies/{vacancyId}/review/{vacancyReviewId}", cancellationToken: cancellationToken);
    }
}