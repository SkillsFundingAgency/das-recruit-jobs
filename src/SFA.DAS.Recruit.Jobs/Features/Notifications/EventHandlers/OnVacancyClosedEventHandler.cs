using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.Services;
using ClosureReason = SFA.DAS.Recruit.Jobs.Domain.ClosureReason;
using VacancyStatus = SFA.DAS.Recruit.Jobs.Domain.VacancyStatus;

namespace SFA.DAS.Recruit.Jobs.Features.Notifications.EventHandlers;

public class OnVacancyClosedEventHandler(
    ILogger<OnVacancyClosedEventHandler> logger,
    IVacancyService vacancyService,
    INotificationService notificationService,
    IQueueClient<NotificationEmail> queueClient) : IHandleMessages<VacancyClosedEvent>
{
    public async Task Handle(VacancyClosedEvent message, IMessageHandlerContext context)
    {
        var vacancy = await vacancyService.GetByIdAsync(message.VacancyId);
        if (vacancy is null)
        {
            logger.LogError("OnVacancyClosedEventHandler: could not find closed vacancy '{VacancyId}' ({VacancyReference})", message.VacancyId, message.VacancyReference);
            return;
        }

        if (vacancy is not { Status: VacancyStatus.Closed, ClosureReason: ClosureReason.WithdrawnByQa })
        {
            return;
        }

        var notifications = await notificationService.CreateVacancyNotificationsAsync(message.VacancyId, context.CancellationToken);
        foreach (var notification in notifications)
        {
            await queueClient.SendMessageAsync(notification, context.CancellationToken);
        }
    }
}