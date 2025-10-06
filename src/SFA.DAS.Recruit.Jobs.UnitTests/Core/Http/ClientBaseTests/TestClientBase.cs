using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Core.Http.ClientBaseTests;

public class TestClientBase(HttpClient httpClient, RecruitJobsOuterApiConfiguration jobsOuterApiConfiguration, JsonSerializerOptions jsonSerializationOptions) : ClientBase(httpClient, jobsOuterApiConfiguration, jsonSerializationOptions)
{
    public async Task<ApiResponse<T>> Get<T>(string url)
    {
        return await GetAsync<T>(url);
    }
    
    public async Task<ApiResponse<T>> Post<T>(string url, object? payload = null)
    {
        return await PostAsync<T>(url, payload);
    }
}