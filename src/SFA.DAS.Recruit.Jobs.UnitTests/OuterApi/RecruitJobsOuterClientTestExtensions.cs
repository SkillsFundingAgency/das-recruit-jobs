using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.OuterApi;
using System.Text.Json;

namespace SFA.DAS.Recruit.Jobs.UnitTests.OuterApi;

internal static class RecruitJobsOuterClientTestExtensions
{
    public static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static RecruitJobsOuterClient CreateSut(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var config = new RecruitJobsOuterApiConfiguration
        {
            BaseUrl = "http://localhost:8080",
            Key = "1234567890"
        };

        return new RecruitJobsOuterClient(httpClient, config, SerializerOptions);
    }
}
