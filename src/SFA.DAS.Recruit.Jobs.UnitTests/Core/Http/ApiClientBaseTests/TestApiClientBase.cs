using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Core.Http.ApiClientBaseTests;

public class TestApiClientBase(HttpClient httpClient, RecruitJobsOuterApiConfiguration jobsOuterApiConfiguration, JsonSerializerOptions jsonSerializationOptions) : ApiClientBase<RecruitJobsOuterApiConfiguration>(httpClient, jobsOuterApiConfiguration, jsonSerializationOptions);