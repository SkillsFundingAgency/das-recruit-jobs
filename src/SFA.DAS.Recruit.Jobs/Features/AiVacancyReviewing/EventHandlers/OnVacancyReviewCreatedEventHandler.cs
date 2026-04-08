using SFA.DAS.Recruit.Api.Core.Events;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;

namespace SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.EventHandlers;

public class OnVacancyReviewCreatedEventHandler(IRecruitAiOuterClient recruitAiOuterClient, IQueueClient<AiVacancyReviewMessage> queueClient) : IHandleMessages<VacancyReviewCreatedEvent>
{
    public async Task Handle(VacancyReviewCreatedEvent message, IMessageHandlerContext context)
    {
        // Create the record in recruit ai inner
        var response = await recruitAiOuterClient.CreateVacancyReviewAsync(message.VacancyId, message.VacancyReviewId, context.CancellationToken);
        if (!response.Success)
        {
            throw new ApiException($"Failed to create the initial ai vacancy review record. Id='{message.VacancyId}', ReviewId='{message.VacancyReviewId}'", response);
        }

        // Queue the message to perform the ai review
        await queueClient.SendMessageAsync(new AiVacancyReviewMessage
        {
            VacancyId = message.VacancyId,
            VacancyReviewId = message.VacancyReviewId,
        }, context.CancellationToken);
    }
}