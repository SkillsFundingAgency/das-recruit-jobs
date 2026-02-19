using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;

namespace SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.EventHandlers;

public class OnVacancyReviewCreatedEventHandler(IRecruitAiOuterClient recruitAiOuterClient, IQueueClient<AiVacancyReviewMessage> queueClient) : IHandleMessages<VacancyReviewCreatedEvent>
{
    private const int MaxRuntimeInSeconds = 30;
    
    public async Task Handle(VacancyReviewCreatedEvent message, IMessageHandlerContext context)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(MaxRuntimeInSeconds));
        
        // Create the record in recruit ai inner
        var response = await recruitAiOuterClient.CreateVacancyReviewAsync(message.VacancyId, message.VacancyReviewId, cancellationTokenSource.Token);
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