using Microsoft.Extensions.Logging;
using SFA.DAS.ProviderRelationships.Messages.Events;
using SFA.DAS.ProviderRelationships.Types.Models;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi.Clients;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.EventHandlers;

public class UpdatedPermissionsExternalSystemEventsHandler(
    ILogger<UpdatedPermissionsExternalSystemEventsHandler> logger,
    IQueueClient<TransferVacanciesFromProviderQueueMessage> queueClient,
    IUpdatedPermissionsClient updatePermissionsClient)
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

        if (message.GrantedOperations.Contains(Operation.Recruitment))
        {
            return;
        }
        
        logger.LogInformation("Transferring vacancies from Provider {Ukprn} to Employer {AccountId}", message.Ukprn, message.AccountId);
        var associationExists = await updatePermissionsClient.VerifyEmployerLegalEntityAssociated(message.AccountId, message.AccountLegalEntityId, context.CancellationToken);
        if (!associationExists)
        {
            throw new Exception($"Could not find matching Account Legal Entity Id {message.AccountLegalEntityId} for Employer Account {message.AccountId}");
        }
        
        await queueClient.SendMessageAsync(new TransferVacanciesFromProviderQueueMessage
        {
            Ukprn = message.Ukprn,
            EmployerAccountId = message.AccountId,
            AccountLegalEntityId = message.AccountLegalEntityId,
            UserRef = message.UserRef.Value,
            UserEmailAddress = message.UserEmailAddress,
            UserName = $"{message.UserFirstName} {message.UserLastName}",
            TransferReason = TransferReason.EmployerRevokedPermission
        });
    }
}