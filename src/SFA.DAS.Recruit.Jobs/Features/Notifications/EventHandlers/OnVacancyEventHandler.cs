using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.Services;

namespace SFA.DAS.Recruit.Jobs.Features.Notifications.EventHandlers;

public class OnVacancyEventHandler(ILogger<OnVacancyEventHandler> logger,
    INotificationService notificationService,
    IQueueClient<NotificationEmail> queueClient) :
    IHandleMessages<VacancyClosedEvent>,
    IHandleMessages<VacancyApprovedEvent>,
    IHandleMessages<VacancyReferredEvent>
{
    private async Task SendNotifications(Guid vacancyId, VacancyStatus? status = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("1. SendNotification for the vacancy: {Id}, Status: {Status}", vacancyId, status);
        
        var notifications = await notificationService.CreateVacancyNotificationsAsync(vacancyId, status, cancellationToken);
        foreach (var notificationEmail in notifications)
        {
            logger.LogInformation("2. Notifications received for template: {templateId} and Email Address: {email}", notificationEmail.TemplateId, notificationEmail.RecipientAddress); 
        }
        foreach (var notification in notifications
                     .DistinctBy(x => new // Ensure we only send one notification per template and recipient address
                     {
                         x.TemplateId,
                         RecipientAddress = x.RecipientAddress.ToLowerInvariant()
                     }))
        {
            notification.RecipientAddress = "Balaji.JAMBULINGAM@education.gov.uk";
            logger.LogInformation("3. Sending email template: {templateId} and Email Address: {email}", notification.TemplateId, notification.RecipientAddress);
            await queueClient.SendMessageAsync(notification, cancellationToken);
        }
    }
    
    public async Task Handle(VacancyClosedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Event Name: {EventName}, MessageId: {MessageId}", nameof(VacancyClosedEvent), context.MessageId);

        await SendNotifications(message.VacancyId, cancellationToken: context.CancellationToken);
    }

    public async Task Handle(VacancyApprovedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Event Name: {EventName}, MessageId: {MessageId}", nameof(VacancyApprovedEvent), context.MessageId);

        await SendNotifications(message.VacancyId, VacancyStatus.Approved, context.CancellationToken);
    }

    public async Task Handle(VacancyReferredEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Event Name: {EventName}, MessageId: {MessageId}", nameof(VacancyReferredEvent), context.MessageId);
        logger.LogInformation("VacancyId: {VacancyId}, VacancyReference: {VacRef}", message.VacancyId, message.VacancyReference);

        foreach (var header in context.MessageHeaders)
        {
            logger.LogInformation("Header {Key}: {Value}", header.Key, header.Value);
        }

        logger.LogInformation(
            "MessageId={MessageId}, OriginatingEndpoint={OriginatingEndpoint}, CorrelationId={CorrelationId}",
            context.MessageId,
            context.MessageHeaders.GetValueOrDefault("NServiceBus.OriginatingEndpoint"),
            context.MessageHeaders.GetValueOrDefault("NServiceBus.CorrelationId"));

        await SendNotifications(message.VacancyId, cancellationToken: context.CancellationToken);
    }
}