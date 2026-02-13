using Microsoft.AspNetCore.WebUtilities;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Metrics;
using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Models;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Analytics;

namespace SFA.DAS.Recruit.Jobs.OuterApi;

public interface IRecruitJobsOuterClient
{
    Task<ApiResponse<List<NotificationEmail>>> GetDelayedNotificationsBatchBeforeDateAsync(DateTime dateTime, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<NotificationEmail>>> GetDelayedNotificationsBatchByUsersInactiveStatus(CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteDelayedNotificationsAsync(IEnumerable<long> ids);
    Task<ApiResponse<VacanciesToClose>> GetVacanciesToCloseAsync(DateTime closingDate, CancellationToken cancellationToken = default);
    Task<ApiResponse<StaleVacancies>> GetDraftVacanciesToCloseAsync(DateTime pointInTime,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<StaleVacancies>> GetEmployerReviewedVacanciesToClose(DateTime pointInTime,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<StaleVacancies>> GetRejectedEmployerVacanciesToClose(DateTime pointInTime,
        CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteVacancyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse> SendEmailAsync(NotificationEmail email, CancellationToken cancellationToken = default);
    Task<ApiResponse<VacancyMetricResponse>> GetVacancyMetricsByDateAsync(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<GetOneVacancyAnalyticsResponse>> GetOneVacancyAnalyticsAsync(long vacancyReference, CancellationToken cancellationToken = default);
    Task<ApiResponse> PutOneVacancyAnalyticsAsync(long vacancyReference, List<VacancyAnalytics> vacancyAnalytics, CancellationToken cancellationToken = default);
}

public class RecruitJobsOuterClient(
    HttpClient httpClient,
    RecruitJobsOuterApiConfiguration jobsOuterApiConfiguration,
    JsonSerializerOptions jsonSerializationOptions)
    : ClientBase(httpClient, jobsOuterApiConfiguration, jsonSerializationOptions), IRecruitJobsOuterClient  
{
    public async Task<ApiResponse<List<NotificationEmail>>> GetDelayedNotificationsBatchBeforeDateAsync(DateTime dateTime, CancellationToken cancellationToken = default)
    {
        const string baseUrl = "delayed-notifications";
        var url = QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            { "dateTime", dateTime.ToString("s") },
        });
        
        return await GetAsync<List<NotificationEmail>>(url, cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse<List<NotificationEmail>>> GetDelayedNotificationsBatchByUsersInactiveStatus(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<NotificationEmail>>("delayed-notifications/users/inactive", cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse> DeleteDelayedNotificationsAsync(IEnumerable<long> ids)
    {
        return await PostAsync<NoResponse>("delayed-notifications/delete", ids);
    }

    public async Task<ApiResponse<VacanciesToClose>> GetVacanciesToCloseAsync(DateTime closingDate, CancellationToken cancellationToken = default)
    {
        const string baseUrl = "vacancies/stale/live";
        var url = QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            { "pointInTime", closingDate.ToString("s") },
        });

        return await GetAsync<VacanciesToClose>(url, cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse<StaleVacancies>> GetDraftVacanciesToCloseAsync(DateTime pointInTime, CancellationToken cancellationToken = default)
    {
        const string baseUrl = "vacancies/stale/draft";
        var url = QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            { "pointInTime", pointInTime.ToString("s") },
        });

        return await GetAsync<StaleVacancies>(url, cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse<StaleVacancies>> GetEmployerReviewedVacanciesToClose(DateTime pointInTime, CancellationToken cancellationToken = default)
    {
        const string baseUrl = "vacancies/stale/employer-reviewed";
        var url = QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            { "pointInTime", pointInTime.ToString("s") },
        });

        return await GetAsync<StaleVacancies>(url, cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse<StaleVacancies>> GetRejectedEmployerVacanciesToClose(DateTime pointInTime, CancellationToken cancellationToken = default)
    {
        const string baseUrl = "vacancies/stale/rejected";
        var url = QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            { "pointInTime", pointInTime.ToString("s") },
        });

        return await GetAsync<StaleVacancies>(url, cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse> DeleteVacancyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await PostAsync<NoResponse>("vacancies/delete", id, cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse> SendEmailAsync(NotificationEmail email, CancellationToken cancellationToken = default)
    {
        return await PostAsync<NoResponse>("delayed-notifications/send", email, cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse<VacancyMetricResponse>> GetVacancyMetricsByDateAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        const string baseUrl = "metrics/vacancies";
        var url = QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            { "startDate", startDate.ToString("s") },
            { "endDate", endDate.ToString("s") },
        });

        return await GetAsync<VacancyMetricResponse>(url, cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse<GetOneVacancyAnalyticsResponse>> GetOneVacancyAnalyticsAsync(long vacancyReference, CancellationToken cancellationToken = default)
    {
        return await GetAsync<GetOneVacancyAnalyticsResponse>($"vacancies/{vacancyReference}/analytics", cancellationToken: cancellationToken);
    }

    public async Task<ApiResponse> PutOneVacancyAnalyticsAsync(long vacancyReference, List<VacancyAnalytics> vacancyAnalytics,
        CancellationToken cancellationToken = default)
    {
        return await PutAsync<NoResponse>($"vacancies/{vacancyReference}/analytics", new PutOneVacancyAnalyticsRequest(vacancyAnalytics), cancellationToken: cancellationToken);
    }
}