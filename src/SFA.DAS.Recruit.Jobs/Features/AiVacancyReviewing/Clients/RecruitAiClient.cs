using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;

public interface IRecruitAiOuterClient
{
    Task<ApiResponse> ReviewVacancyAsync(Guid vacancyId, Guid vacancyReviewId, CancellationToken cancellationToken);
    Task<ApiResponse> CreateVacancyReviewAsync(Guid vacancyId, Guid vacancyReviewId, AiReviewStatus reviewStatus, CancellationToken cancellationToken);
    Task<ApiResponse> SendVacancyForManualReviewAsync(Guid vacancyId, Guid vacancyReviewId, CancellationToken cancellationToken);
    Task<ApiResponse> AutoApproveVacancyAsync(Guid vacancyId, Guid vacancyReviewId, CancellationToken cancellationToken);
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

    public async Task<ApiResponse> CreateVacancyReviewAsync(Guid vacancyId, Guid vacancyReviewId, AiReviewStatus reviewStatus, CancellationToken cancellationToken)
    {
        return await PostAsync<NoResponse>(
            $"ai/vacancies/{vacancyId}/review/{vacancyReviewId}",
            new CreateVacancyReviewData(reviewStatus),
            cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse> SendVacancyForManualReviewAsync(Guid vacancyId, Guid vacancyReviewId, CancellationToken cancellationToken)
    {
        return await PostAsync<NoResponse>($"ai/vacancies/{vacancyId}/refer-to-manual", vacancyReviewId, cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse> AutoApproveVacancyAsync(Guid vacancyId, Guid vacancyReviewId, CancellationToken cancellationToken)
    {
        return await PostAsync<NoResponse>($"ai/vacancies/{vacancyId}/approve", vacancyReviewId, cancellationToken: cancellationToken);
    }
}