using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Clients;

public interface IUpdatedPermissionsClient
{
    Task<List<Guid>> GetProviderVacanciesToTransfer(long ukprn, long accountLegalEntityId, CancellationToken cancellationToken = default);
    Task TransferVacancyAsync(Guid vacancyId, TransferReason transferReason, CancellationToken cancellationToken);
}

public class UpdatedPermissionsClient(
    HttpClient httpClient,
    RecruitJobsOuterApiConfiguration jobsOuterApiConfiguration,
    JsonSerializerOptions jsonSerializationOptions)
    : ClientBase(httpClient, jobsOuterApiConfiguration, jsonSerializationOptions), IUpdatedPermissionsClient
{
    public async Task<List<Guid>> GetProviderVacanciesToTransfer(
        long ukprn,
        long accountLegalEntityId,
        CancellationToken cancellationToken = default)
    {
        const string baseUrl = "updated-employer-permissions/vacancies/transferable";
        var url = QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            { "ukprn", ukprn.ToString() },
            { "accountLegalEntityId", accountLegalEntityId.ToString() },
        });
        
        var response = await GetAsync<List<Guid>>(url, cancellationToken: cancellationToken);
        return response.Success
            ? response.Payload ?? []
            : throw new ApiException("Failed to retrieve list of vacancies to transfer", response); 
    }

    public async Task TransferVacancyAsync(
        Guid vacancyId,
        TransferReason transferReason,
        CancellationToken cancellationToken)
    {
        var url = $"updated-employer-permissions/vacancies/{vacancyId}/transfer";
        var payload = new TransferVacancyRequest(transferReason);
        var response = await PostAsync<NoResponse>(url, payload, cancellationToken: cancellationToken);
        if (!response.Success)
        {
            throw new ApiException($"Failed to transfer vacancy '{vacancyId}'", response);
        }
    }
}