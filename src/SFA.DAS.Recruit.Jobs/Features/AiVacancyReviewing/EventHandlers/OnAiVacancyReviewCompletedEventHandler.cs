using SFA.DAS.RAA.Vacancy.AI.Api.Core.Events;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.EventHandlers;

public class OnAiVacancyReviewCompletedEventHandler(IRecruitAiOuterClient recruitAiOuterClient) : IHandleMessages<AiVacancyReviewCompletedEvent>
{
    public async Task Handle(AiVacancyReviewCompletedEvent message, IMessageHandlerContext context)
    {
        if (message is { ManualReviewRequired: false, ReviewStatus: AiReviewStatus.Passed })
        {
            var approveResponse = await recruitAiOuterClient.AutoApproveVacancyAsync(message.VacancyId, message.VacancyReviewId, context.CancellationToken);
            if (approveResponse.Success)
            {
                return;
            }

            throw new ApiException($"Failed to auto approve vacancy. Id='{message.VacancyId}', ReviewId='{message.VacancyReviewId}'", approveResponse);
        }

        var referResponse = await recruitAiOuterClient.SendVacancyForManualReviewAsync(message.VacancyId, message.VacancyReviewId, context.CancellationToken);
        if (referResponse.Success)
        {
            return;
        }

        throw new ApiException($"Failed to send vacancy for manual review. Id='{message.VacancyId}', ReviewId='{message.VacancyReviewId}'", referResponse);
    }
}