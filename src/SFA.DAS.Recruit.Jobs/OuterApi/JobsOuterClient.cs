using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi;

public interface IJobsOuterClient : IApiClient;

public class JobsOuterClient(
    HttpClient httpClient,
    RecruitJobsOuterApiConfiguration jobsOuterApiConfiguration,
    JsonSerializerOptions jsonSerializationOptions)
    : ApiClientBase<RecruitJobsOuterApiConfiguration>(httpClient, jobsOuterApiConfiguration, jsonSerializationOptions), IJobsOuterClient;