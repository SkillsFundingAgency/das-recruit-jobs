using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Events;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.EventHandlers;

public class OnAiVacancyReviewCompletedEventHandler(
    ILogger<OnAiVacancyReviewCompletedEventHandler> logger,
    IRecruitAiOuterClient recruitAiOuterClient,
    IVariantFeatureManager featureManager) : IHandleMessages<AiVacancyReviewCompletedEvent>
{
    public async Task Handle(AiVacancyReviewCompletedEvent message, IMessageHandlerContext context)
    {
        var aiReviewsEnabled = await featureManager.IsEnabledAsync(FeatureFlags.AiReviews, context.CancellationToken);
        if (aiReviewsEnabled && message is { ManualReviewRequired: false, ReviewStatus: AiReviewStatus.Passed })
        {
            var approveResponse = await recruitAiOuterClient.AutoApproveVacancyAsync(message.VacancyId, message.VacancyReviewId, context.CancellationToken);
            if (approveResponse.Success)
            {
                return;
            }

            throw new ApiException($"Failed to auto approve vacancy. Id='{message.VacancyId}', ReviewId='{message.VacancyReviewId}'", approveResponse);
        }

        if (!aiReviewsEnabled)
        {
            logger.LogInformation("AI Reviews disabled, sending vacancy '{VacancyId}' for manual review", message.VacancyId);
        }
        
        var referResponse = await recruitAiOuterClient.SendVacancyForManualReviewAsync(message.VacancyId, message.VacancyReviewId, context.CancellationToken);
        if (referResponse.Success)
        {
            return;
        }

        throw new ApiException($"Failed to send vacancy for manual review. Id='{message.VacancyId}', ReviewId='{message.VacancyReviewId}'", referResponse);
    }
}