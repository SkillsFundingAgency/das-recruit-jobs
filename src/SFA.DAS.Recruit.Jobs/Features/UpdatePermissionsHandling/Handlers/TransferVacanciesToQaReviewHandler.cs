using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;
using SFA.DAS.Recruit.Jobs.OuterApi.Responses;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;

public interface ITransferVacanciesToQaReviewHandler
{
    Task RunAsync(TransferVacanciesFromEmployerReviewToQaReviewQueueMessage message, CancellationToken cancellationToken);    
}

internal class TransferVacanciesToQaReviewHandler(
    IJobsOuterClient jobsOuterClient,
    IQueueClient<TransferVacancyFromEmployerReviewToQaReviewQueueMessage> queueClient,
    IEncodingService encodingService): ITransferVacanciesToQaReviewHandler
{
    public async Task RunAsync(TransferVacanciesFromEmployerReviewToQaReviewQueueMessage message, CancellationToken cancellationToken)
    {
        // fetch the vacancies to transfer
        var accountLegalEntityId = encodingService.Decode(message.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);
        var response = await jobsOuterClient.GetAsync<GetProviderOwnedVacanciesInReviewResponse>(new GetProviderOwnedVacanciesInReviewRequest(message.Ukprn, accountLegalEntityId), cancellationToken);
        response.ThrowIfErrored("Failed to fetch Provider owned vacancies in review for transfer to QA");

        if (response.Payload is null)
        {
            return;
        }

        // send a message to transfer each one
        foreach (var vacancyId in response.Payload)
        {
            await queueClient.SendMessageAsync(new TransferVacancyFromEmployerReviewToQaReviewQueueMessage(vacancyId, message.UserRef, message.UserName, message.UserEmailAddress), cancellationToken);
        }
    }
}