using Microsoft.Extensions.Logging;
using SFA.DAS.ProviderRelationships.Messages.Events;
using SFA.DAS.ProviderRelationships.Types.Models;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;
using SFA.DAS.Recruit.Jobs.OuterApi.Responses;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.EventHandlers;

public class UpdatedPermissionsExternalSystemEventsHandler(
    ILogger<UpdatedPermissionsExternalSystemEventsHandler> logger,
    IQueueClient<TransferVacanciesFromProviderQueueMessage> transferToLegalEntityQueueClient,
    IQueueClient<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage> transferToQaReviewQueueClient,
    IJobsOuterClient jobsOuterClient)
    : IHandleMessages<UpdatedPermissionsEvent>
{
    public async Task Handle(UpdatedPermissionsEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Attempting to process {EventName} : {EventMessage}", nameof(UpdatedPermissionsEvent), message);
        if (!message.UserRef.HasValue)
        {
            logger.LogInformation("Not handling Provider {OperationName} Permission being revoked as it is a consequence of Provider being blocked by QA on Recruit.", nameof(Operation.Recruitment));
            return;
        }

        if (!message.GrantedOperations.Contains(Operation.Recruitment))
        {
            logger.LogInformation("Transferring vacancies from Provider {Ukprn} to Employer {AccountId}", message.Ukprn, message.AccountId);
        
            await transferToLegalEntityQueueClient.SendMessageAsync(new TransferVacanciesFromProviderQueueMessage
            {
                Ukprn = message.Ukprn,
                AccountLegalEntityId = message.AccountLegalEntityId,
                TransferReason = TransferReason.EmployerRevokedPermission
            }, context.CancellationToken);
        } else if (!message.GrantedOperations.Contains(Operation.RecruitmentRequiresReview))
        {
            logger.LogInformation("Transferring vacancies from Employer Review to QA Review for Provider {Ukprn}", message.Ukprn);
            var response = await jobsOuterClient.GetAsync<GetAccountLegalEntitiesResponse>(new GetAccountLegalEntitiesRequest(message.AccountId), context.CancellationToken);
            response.ThrowIfErrored();

            var legalEntity = response.Payload?.AccountLegalEntities.FirstOrDefault(x => x.AccountLegalEntityId == message.AccountLegalEntityId);
            if (legalEntity is null)
            {
                throw new Exception($"Could not find matching Account Legal Entity Id {message.AccountLegalEntityId} for Employer Account {message.AccountId}");
            }
        
            await transferToQaReviewQueueClient.SendMessageAsync(new TransferVacanciesFromEmployerReviewToQaReviewQueueMessage
            {
                Ukprn = message.Ukprn,
                AccountLegalEntityPublicHashedId = legalEntity.AccountLegalEntityPublicHashedId,
                UserRef = message.UserRef!.Value,
                UserEmailAddress = message.UserEmailAddress,
                UserName = $"{message.UserFirstName} {message.UserLastName}"
            }, context.CancellationToken);
        }
    }
}