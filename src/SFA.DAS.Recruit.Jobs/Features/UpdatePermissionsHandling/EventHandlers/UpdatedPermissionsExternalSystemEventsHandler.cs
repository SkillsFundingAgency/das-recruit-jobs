using Microsoft.Extensions.Logging;
using SFA.DAS.ProviderRelationships.Messages.Events;
using SFA.DAS.ProviderRelationships.Types.Models;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.EventHandlers;

public class UpdatedPermissionsExternalSystemEventsHandler(
    ILogger<UpdatedPermissionsExternalSystemEventsHandler> logger,
    IQueueClient<TransferVacanciesFromProviderQueueMessage> queueClient)
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
        
        await queueClient.SendMessageAsync(new TransferVacanciesFromProviderQueueMessage
        {
            Ukprn = message.Ukprn,
            AccountLegalEntityId = message.AccountLegalEntityId,
            TransferReason = TransferReason.EmployerRevokedPermission
        });
    }
}