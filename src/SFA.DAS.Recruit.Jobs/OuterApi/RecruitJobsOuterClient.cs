using Microsoft.AspNetCore.WebUtilities;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using System.Text.Json;

namespace SFA.DAS.Recruit.Jobs.OuterApi;

public interface IRecruitJobsOuterClient
{
    Task<ApiResponse<List<NotificationEmail>>> GetDelayedNotificationsBatchBeforeDateAsync(DateTime dateTime, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<NotificationEmail>>> GetDelayedNotificationsBatchByUsersInactiveStatus(CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteDelayedNotificationsAsync(IEnumerable<long> ids);
    Task<ApiResponse<VacanciesToClose>> GetVacanciesToCloseAsync(DateTime closingDate, CancellationToken cancellationToken = default);
    Task<ApiResponse> SendEmailAsync(NotificationEmail email, CancellationToken cancellationToken = default);
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
}