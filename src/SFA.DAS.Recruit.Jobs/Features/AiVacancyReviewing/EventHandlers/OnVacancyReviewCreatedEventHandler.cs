using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;

namespace SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.EventHandlers;

// nservicebus not setup yet
public class OnVacancyReviewCreatedEventHandler(IRecruitAiOuterClient recruitAiOuterClient, IQueueClient<AiVacancyReviewMessage> queueClient)//: IHandleMessages<VacancyReviewCreatedEvent>
{
    //public async Task HandleAsync(VacancyReviewCreatedEvent message, IMessageHandlerContext context, CancellationToken cancellationToken)
    public async Task Handle(VacancyReviewCreatedEvent message, CancellationToken cancellationToken)
    {
        // Create the record in recruit ai inner
        var response = await recruitAiOuterClient.CreateVacancyReviewAsync(message.VacancyId, message.VacancyReviewId, cancellationToken);
        if (!response.Success)
        {
            throw new ApiException("Failed to create the initial ai vacancy review record", response);
        }

        // Queue the message to perform the ai review
        await queueClient.SendMessageAsync(new AiVacancyReviewMessage
        {
            VacancyId = message.VacancyId,
            VacancyReviewId = message.VacancyReviewId,
        });
    }
}