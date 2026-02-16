using Microsoft.AspNetCore.WebUtilities;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Metrics;
using System.Text.Json;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Analytics;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Close;
using VacancyAnalytics = SFA.DAS.Recruit.Jobs.Core.Models.VacancyAnalytics;

namespace SFA.DAS.Recruit.Jobs.OuterApi;

public interface IRecruitJobsOuterClient
{
    Task<ApiResponse<List<NotificationEmail>>> GetDelayedNotificationsBatchBeforeDateAsync(DateTime dateTime, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<NotificationEmail>>> GetDelayedNotificationsBatchByUsersInactiveStatus(CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteDelayedNotificationsAsync(IEnumerable<long> ids);
    Task<ApiResponse<VacanciesToClose>> GetVacanciesToCloseAsync(DateTime closingDate, CancellationToken cancellationToken = default);
    Task<ApiResponse> SendEmailAsync(NotificationEmail email, CancellationToken cancellationToken = default);
    Task<ApiResponse<VacancyMetricResponse>> GetVacancyMetricsByDateAsync(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<GetOneVacancyAnalyticsResponse>> GetOneVacancyAnalyticsAsync(long vacancyReference, CancellationToken cancellationToken = default);
    Task<ApiResponse> PutOneVacancyAnalyticsAsync(long vacancyReference, List<VacancyAnalytics> vacancyAnalytics, CancellationToken cancellationToken = default);
    Task<ApiResponse> PostVacancyToClose(Guid id, long vacancyReference, ClosureReason reason, CancellationToken cancellationToken = default);
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
        const string baseUrl = "vacancies/getVacanciesToClose";
        var url = QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            { "pointInTime", closingDate.ToString("s") },
        });

        return await GetAsync<VacanciesToClose>(url, cancellationToken: cancellationToken);
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

    public async Task<ApiResponse> PostVacancyToClose(Guid id, long vacancyReference, ClosureReason reason, CancellationToken cancellationToken = default)
    {
        return await PostAsync<NoResponse>($"vacancies/{vacancyReference}/close",
            new PostOneVacancyCloseRequest(id, reason), cancellationToken: cancellationToken);
    }
}