using Esfa.Recruit.Vacancies.Client.Domain.Events;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.Services;

namespace SFA.DAS.Recruit.Jobs.Features.Notifications.EventHandlers;

public class OnVacancyEventHandler(INotificationService notificationService, IQueueClient<NotificationEmail> queueClient) :
    IHandleMessages<VacancyClosedEvent>,
    IHandleMessages<VacancyApprovedEvent>,
    IHandleMessages<VacancyReferredEvent>
{
    private async Task SendNotifications(Guid vacancyId, CancellationToken cancellationToken)
    {
        var notifications = await notificationService.CreateVacancyNotificationsAsync(vacancyId, cancellationToken);
        foreach (var notification in notifications)
        {
            await queueClient.SendMessageAsync(notification, cancellationToken);
        }
    }
    
    public async Task Handle(VacancyClosedEvent message, IMessageHandlerContext context)
    {
        await SendNotifications(message.VacancyId, context.CancellationToken);
    }

    public async Task Handle(VacancyApprovedEvent message, IMessageHandlerContext context)
    {
        await SendNotifications(message.VacancyId, context.CancellationToken);
    }

    public async Task Handle(VacancyReferredEvent message, IMessageHandlerContext context)
    {
        await SendNotifications(message.VacancyId, context.CancellationToken);
    }
}