using Esfa.Recruit.Vacancies.Client.Domain.Events;
using SFA.DAS.Recruit.Api.Core.Events;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.Services;

namespace SFA.DAS.Recruit.Jobs.Features.Notifications.EventHandlers;

public class OnVacancyEventHandler(INotificationService notificationService, IQueueClient<NotificationEmail> queueClient) :
    IHandleMessages<VacancyClosedEvent>,
    IHandleMessages<VacancyApprovedEvent>,
    IHandleMessages<VacancyReferredEvent>,
    IHandleMessages<VacancySubmittedEvent>
{
    private async Task SendNotifications(Guid vacancyId, VacancyStatus? status = null, CancellationToken cancellationToken = default)
    {
        var notifications = await notificationService.CreateVacancyNotificationsAsync(vacancyId, status, cancellationToken);
        foreach (var notification in notifications)
        {
            await queueClient.SendMessageAsync(notification, cancellationToken);
        }
    }
    
    public async Task Handle(VacancyClosedEvent message, IMessageHandlerContext context)
    {
        await SendNotifications(message.VacancyId, cancellationToken: context.CancellationToken);
    }

    public async Task Handle(VacancyApprovedEvent message, IMessageHandlerContext context)
    {
        await SendNotifications(message.VacancyId, VacancyStatus.Approved, context.CancellationToken);
    }

    public async Task Handle(VacancyReferredEvent message, IMessageHandlerContext context)
    {
        await SendNotifications(message.VacancyId, cancellationToken: context.CancellationToken);
    }

    public async Task Handle(VacancySubmittedEvent message, IMessageHandlerContext context)
    {
        await SendNotifications(message.VacancyId, VacancyStatus.Submitted, cancellationToken: context.CancellationToken);
    }
}