using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.OuterApi;

public interface IRecruitJobsOuterClient
{
    Task<ApiResponse<List<NotificationEmail>>> GetDelayedNotificationsBatchBeforeDateAsync(DateTime dateTime, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteDelayedNotificationsAsync(IEnumerable<long> ids);
    Task<ApiResponse> SendEmailAsync(NotificationEmail email, CancellationToken cancellationToken = default);
}

public class RecruitJobsOuterClient(HttpClient httpClient, RecruitJobsOuterApiConfiguration jobsOuterApiConfiguration, JsonSerializerOptions jsonSerializationOptions) : ClientBase(httpClient, jobsOuterApiConfiguration, jsonSerializationOptions), IRecruitJobsOuterClient  
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

    public async Task<ApiResponse> DeleteDelayedNotificationsAsync(IEnumerable<long> ids)
    {
        return await PostAsync<NoResponse>("delayed-notifications/delete", ids);
    }

    public async Task<ApiResponse> SendEmailAsync(NotificationEmail email, CancellationToken cancellationToken = default)
    {
        return await PostAsync<NoResponse>("delayed-notifications/send", email, cancellationToken: cancellationToken);
    }
}