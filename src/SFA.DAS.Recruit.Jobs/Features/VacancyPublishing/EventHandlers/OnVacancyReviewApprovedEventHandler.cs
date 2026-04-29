using SFA.DAS.Recruit.Api.Core.Events;
using SFA.DAS.Recruit.Jobs.Features.VacancyPublishing.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyPublishing.EventHandlers;

public class OnVacancyReviewApprovedEventHandler(IVacancyReviewApprovedHandler handler) : IHandleMessages<VacancyReviewApprovedEvent>
{
    public async Task Handle(VacancyReviewApprovedEvent message, IMessageHandlerContext context)
    {
        await handler.HandleAsync(message.VacancyId, context.CancellationToken);
    }
}