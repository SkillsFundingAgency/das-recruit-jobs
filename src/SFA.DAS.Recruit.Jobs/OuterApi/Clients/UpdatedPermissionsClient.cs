using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Clients;

public interface IUpdatedPermissionsClient
{
    Task<bool> VerifyEmployerLegalEntityAssociated(long employerAccountId, long accountLegalEntityId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetProviderVacanciesToTransfer(long ukprn, long employerAccountId, long accountLegalEntityId, CancellationToken cancellationToken = default);
    Task TransferVacancyAsync(Guid vacancyId, Guid userRef, string userEmailAddress, string userName, TransferReason transferReason, CancellationToken cancellationToken);
}

public class UpdatedPermissionsClient(
    HttpClient httpClient,
    RecruitJobsOuterApiConfiguration jobsOuterApiConfiguration,
    JsonSerializerOptions jsonSerializationOptions)
    : ClientBase(httpClient, jobsOuterApiConfiguration, jsonSerializationOptions), IUpdatedPermissionsClient
{
    public async Task<bool> VerifyEmployerLegalEntityAssociated(
        long employerAccountId,
        long accountLegalEntityId,
        CancellationToken cancellationToken = default)
    {
        var url = $"updated-employer-permissions/employers/{employerAccountId}/legal-entities/{accountLegalEntityId}";
        var response = await GetAsync<bool>(url, cancellationToken: cancellationToken);
        return response.Success
            ? response.Payload
            : throw new ApiException("Failed to retrieve the account's legal entity", response);
    }

    public async Task<List<Guid>> GetProviderVacanciesToTransfer(
        long ukprn,
        long employerAccountId,
        long accountLegalEntityId,
        CancellationToken cancellationToken = default)
    {
        const string baseUrl = "updated-employer-permissions/vacancies/transferable";
        var url = QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            { "ukprn", ukprn.ToString() },
            { "employerAccountId", employerAccountId.ToString() },
            { "accountLegalEntityId", accountLegalEntityId.ToString() },
        });
        
        var response = await GetAsync<List<Guid>>(url, cancellationToken: cancellationToken);
        return response.Success
            ? response.Payload ?? []
            : throw new ApiException("Failed to retrieve list of vacancies to transfer", response); 
    }

    public async Task TransferVacancyAsync(
        Guid vacancyId,
        Guid userRef,
        string userEmailAddress,
        string userName,
        TransferReason transferReason,
        CancellationToken cancellationToken)
    {
        var url = $"updated-employer-permissions/vacancies/{vacancyId}/transfer";
        var payload = new TransferVacancyRequest(userRef, userEmailAddress, userName, transferReason);
        var response = await PostAsync<NoResponse>(url, payload, cancellationToken: cancellationToken);
        if (!response.Success)
        {
            throw new ApiException($"Failed to transfer vacancy '{vacancyId}'", response);
        }
    }
}