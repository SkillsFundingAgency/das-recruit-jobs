using Esfa.Recruit.Vacancies.Client.Domain.Events;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.Features.Notifications.EventHandlers;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.Services;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.Notifications.EventHandlers;

public class WhenHandlingVacancyEvent
{
    [Test, MoqAutoData]
    public async Task Then_The_Vacancy_Approved_Event_Is_Handled(
        Guid id,
        IMessageHandlerContext context,
        List<NotificationEmail> notifications,
        [Frozen] Mock<INotificationService> notificationService,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Greedy] OnVacancyEventHandler sut)
    {
        // arrange
        notificationService
            .Setup(x => x.CreateVacancyNotificationsAsync(id, VacancyStatus.Approved, context.CancellationToken))
            .ReturnsAsync(notifications);

        // act
        await sut.Handle(new VacancyApprovedEvent { VacancyId = id }, context);

        // assert
        notificationService.Verify(x => x.CreateVacancyNotificationsAsync(id, VacancyStatus.Approved, context.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), context.CancellationToken), Times.Exactly(notifications.Count));
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Vacancy_Referred_Event_Is_Handled(
        Guid id,
        IMessageHandlerContext context,
        List<NotificationEmail> notifications,
        [Frozen] Mock<INotificationService> notificationService,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Greedy] OnVacancyEventHandler sut)
    {
        // arrange
        notificationService
            .Setup(x => x.CreateVacancyNotificationsAsync(id, null, context.CancellationToken))
            .ReturnsAsync(notifications);

        // act
        await sut.Handle(new VacancyReferredEvent { VacancyId = id }, context);

        // assert
        notificationService.Verify(x => x.CreateVacancyNotificationsAsync(id, null, context.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), context.CancellationToken), Times.Exactly(notifications.Count));
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Vacancy_Closed_Event_Is_Handled(
        Guid id,
        IMessageHandlerContext context,
        List<NotificationEmail> notifications,
        [Frozen] Mock<INotificationService> notificationService,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Greedy] OnVacancyEventHandler sut)
    {
        // arrange
        notificationService
            .Setup(x => x.CreateVacancyNotificationsAsync(id, null, context.CancellationToken))
            .ReturnsAsync(notifications);

        // act
        await sut.Handle(new VacancyClosedEvent { VacancyId = id }, context);

        // assert
        notificationService.Verify(x => x.CreateVacancyNotificationsAsync(id, null, context.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), context.CancellationToken), Times.Exactly(notifications.Count));
    }

    [Test, MoqAutoData]
    public async Task Then_The_Duplicate_Message_Vacancy_Closed_Event_Is_Handled(
        Guid id,
        Guid templateId,
        string recipientAddress,
        IMessageHandlerContext context,
        List<NotificationEmail> notifications,
        [Frozen] Mock<INotificationService> notificationService,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Greedy] OnVacancyEventHandler sut)
    {
        // arrange
        foreach (var notification in notifications)
        {
            notification.TemplateId = templateId;
            notification.RecipientAddress = recipientAddress;
        }

        notificationService
            .Setup(x => x.CreateVacancyNotificationsAsync(id, null, context.CancellationToken))
            .ReturnsAsync(notifications);

        // act
        await sut.Handle(new VacancyClosedEvent { VacancyId = id }, context);

        // assert
        notificationService.Verify(x => x.CreateVacancyNotificationsAsync(id, null, context.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), context.CancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Then_The_Duplicate_Message_Vacancy_Approved_Event_Is_Handled(
        Guid id,
        Guid templateId,
        string recipientAddress,
        IMessageHandlerContext context,
        List<NotificationEmail> notifications,
        [Frozen] Mock<INotificationService> notificationService,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Greedy] OnVacancyEventHandler sut)
    {
        // arrange
        foreach (var notification in notifications)
        {
            notification.TemplateId = templateId;
            notification.RecipientAddress = recipientAddress;
        }

        notificationService
            .Setup(x => x.CreateVacancyNotificationsAsync(id, VacancyStatus.Approved, context.CancellationToken))
            .ReturnsAsync(notifications);

        // act
        await sut.Handle(new VacancyApprovedEvent { VacancyId = id }, context);

        // assert
        notificationService.Verify(x => x.CreateVacancyNotificationsAsync(id, VacancyStatus.Approved, context.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), context.CancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Then_The_Duplicate_Message_Vacancy_Referred_Event_Is_Handled(
        Guid id,
        Guid templateId,
        string recipientAddress,
        IMessageHandlerContext context,
        List<NotificationEmail> notifications,
        [Frozen] Mock<INotificationService> notificationService,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Greedy] OnVacancyEventHandler sut)
    {
        // arrange
        foreach (var notification in notifications)
        {
            notification.TemplateId = templateId;
            notification.RecipientAddress = recipientAddress;
        }

        notificationService
            .Setup(x => x.CreateVacancyNotificationsAsync(id, null, context.CancellationToken))
            .ReturnsAsync(notifications);

        // act
        await sut.Handle(new VacancyReferredEvent { VacancyId = id }, context);

        // assert
        notificationService.Verify(x => x.CreateVacancyNotificationsAsync(id, null, context.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), context.CancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Then_The_Duplicate_Message_With_Same_RecipientEmailAddress_Vacancy_Referred_Event_Is_Handled(
        Guid id,
        Guid templateId,
        IMessageHandlerContext context,
        [Frozen] Mock<INotificationService> notificationService,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Greedy] OnVacancyEventHandler sut)
    {
        // arrange
        var notifications = new List<NotificationEmail>
        {
            new NotificationEmail
            {
                TemplateId = templateId,
                RecipientAddress = "some@email.com",
                Tokens = new Dictionary<string, string>(),
                SourceIds = new List<long>()
            },
            new NotificationEmail
            {
                TemplateId = templateId,
                RecipientAddress = "SoME@EmAiL.cOm",
                Tokens = new Dictionary<string, string>(),
                SourceIds = new List<long>()
            },
        };

        notificationService
            .Setup(x => x.CreateVacancyNotificationsAsync(id, null, context.CancellationToken))
            .ReturnsAsync(notifications);

        // act
        await sut.Handle(new VacancyReferredEvent { VacancyId = id }, context);

        // assert
        notificationService.Verify(x => x.CreateVacancyNotificationsAsync(id, null, context.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), context.CancellationToken), Times.Once);
    }
}