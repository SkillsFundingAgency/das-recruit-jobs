using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Api.Core.Events;
using SFA.DAS.Recruit.Jobs.Features.VacancyGeocoding.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyGeocoding.EventHandlers;

public class OnVacancySubmittedEvent(ILogger<OnVacancySubmittedEvent> logger, IGeocodeVacancyHandler handler): IHandleMessages<VacancySubmittedEvent>
{
    public async Task Handle(VacancySubmittedEvent message, IMessageHandlerContext context)
    {
        try
        {
            await handler.HandleAsync(message.VacancyId, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception attempting to geocode vacancy {VacancyId}", message.VacancyId);
        }
    }
}