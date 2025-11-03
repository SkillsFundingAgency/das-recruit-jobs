using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Clients;

public interface IUpdatedPermissionsClient
{
    Task<string> VerifyAccountLegalEntityAsync(string employerAccountId, long messageAccountLegalEntityId, CancellationToken cancellationToken = default);
    Task<List<long>> GetProviderVacanciesToTransfer(long ukprn, string employerAccountId, string accountLegalEntityPublicHashId, CancellationToken cancellationToken = default);
    Task TransferVacancyAsync(long vacancyReference, Guid userRef, string userEmailAddress, string userName, TransferReason transferReason, CancellationToken cancellationToken);
}

public class UpdatedPermissionsClient(
    HttpClient httpClient,
    RecruitJobsOuterApiConfiguration jobsOuterApiConfiguration,
    JsonSerializerOptions jsonSerializationOptions)
    : ClientBase(httpClient, jobsOuterApiConfiguration, jsonSerializationOptions), IUpdatedPermissionsClient
{
    public async Task<string> VerifyAccountLegalEntityAsync(
        string employerAccountId,
        long accountLegalEntityId,
        CancellationToken cancellationToken = default)
    {
        var url = $"updated-employer-permissions/employer-account/{employerAccountId}/legal-entity/{accountLegalEntityId}";
        var response = await GetAsync<string?>(url, cancellationToken: cancellationToken);
        return response.Success
            ? response.Payload!
            : throw new ApiException("Failed to retrieve the account's legal entity", response);
    }

    public async Task<List<long>> GetProviderVacanciesToTransfer(
        long ukprn,
        string employerAccountId,
        string accountLegalEntityPublicHashId,
        CancellationToken cancellationToken = default)
    {
        const string baseUrl = "updated-employer-permissions/vacancies-to-transfer";
        var url = QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            { "ukprn", ukprn.ToString() },
            { "employerAccountId", employerAccountId },
            { "accountLegalEntityPublicHashId", accountLegalEntityPublicHashId },
        });
        
        var response = await GetAsync<List<long>>(url, cancellationToken: cancellationToken);
        return response.Success
            ? response.Payload ?? []
            : throw new ApiException("Failed to retrieve list of vacancies to transfer", response); 
    }

    public async Task TransferVacancyAsync(
        long vacancyReference,
        Guid userRef,
        string userEmailAddress,
        string userName,
        TransferReason transferReason,
        CancellationToken cancellationToken)
    {
        const string url = "updated-employer-permissions/transfer-vacancy";
        var payload = new TransferVacancyRequest(vacancyReference, userRef, userEmailAddress, userName, transferReason);
        var response = await PostAsync<NoResponse>(url, payload, cancellationToken: cancellationToken);
        if (!response.Success)
        {
            throw new ApiException($"Failed to transfer vacancy '{vacancyReference}'", response);
        }
    }
}