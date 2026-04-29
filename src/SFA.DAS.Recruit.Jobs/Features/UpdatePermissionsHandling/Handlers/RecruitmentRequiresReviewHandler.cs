using SFA.DAS.ProviderRelationships.Messages.Events;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;
using SFA.DAS.Recruit.Jobs.OuterApi.Responses;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Handlers;

public interface IRecruitmentRequiresReviewHandler
{
    Task RunAsync(UpdatedPermissionsEvent message, CancellationToken cancellationToken);    
}

public class RecruitmentRequiresReviewHandler(IJobsOuterClient jobsOuterClient, IQueueClient<TransferVacanciesFromEmployerReviewToQaReviewQueueMessage> queueClient): IRecruitmentRequiresReviewHandler
{
    public async Task RunAsync(UpdatedPermissionsEvent message, CancellationToken cancellationToken)
    {
        var response = await jobsOuterClient.GetAsync<GetAccountLegalEntitiesResponse>(new GetAccountLegalEntitiesRequest(message.AccountId), cancellationToken);
        response.ThrowIfErrored();

        var legalEntity = response.Payload?.AccountLegalEntities.FirstOrDefault(x => x.AccountLegalEntityId == message.AccountLegalEntityId);
        if (legalEntity is null)
        {
            throw new Exception($"Could not find matching Account Legal Entity Id {message.AccountLegalEntityId} for Employer Account {message.AccountId}");
        }
        
        await queueClient.SendMessageAsync(new TransferVacanciesFromEmployerReviewToQaReviewQueueMessage
        {
            Ukprn = message.Ukprn,
            AccountLegalEntityPublicHashedId = legalEntity.AccountLegalEntityPublicHashedId,
            UserRef = message.UserRef!.Value,
            UserEmailAddress = message.UserEmailAddress,
            UserName = $"{message.UserFirstName} {message.UserLastName}"
        }, cancellationToken);
    }
}