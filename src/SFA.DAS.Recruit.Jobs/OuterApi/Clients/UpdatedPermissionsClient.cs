using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Clients;

public interface IUpdatedPermissionsClient
{
    Task<string?> VerifyAccountLegalEntityAsync(string employerAccountId, long messageAccountLegalEntityId, CancellationToken cancellationToken = default);
}

public class UpdatedPermissionsClient(
    HttpClient httpClient,
    RecruitJobsOuterApiConfiguration jobsOuterApiConfiguration,
    JsonSerializerOptions jsonSerializationOptions)
    : ClientBase(httpClient, jobsOuterApiConfiguration, jsonSerializationOptions), IUpdatedPermissionsClient
{
    public async Task<string?> VerifyAccountLegalEntityAsync(
        string employerAccountId,
        long accountLegalEntityId,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"updated-employer-permissions/employer-account/{employerAccountId}/legal-entity/{accountLegalEntityId}";
        var response = await GetAsync<string?>(url, cancellationToken: cancellationToken);
        return response.Success
            ? response.Payload
            : null;
    }
}