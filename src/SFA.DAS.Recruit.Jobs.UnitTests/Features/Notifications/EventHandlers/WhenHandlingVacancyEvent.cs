using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using SFA.DAS.Recruit.Api.Core.Events;
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
            .Setup(x => x.CreateVacancyNotificationsAsync(id, context.CancellationToken))
            .ReturnsAsync(notifications);

        // act
        await sut.Handle(new VacancyClosedEvent { VacancyId = id }, context);

        // assert
        notificationService.Verify(x => x.CreateVacancyNotificationsAsync(id, null, context.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), context.CancellationToken), Times.Exactly(notifications.Count));
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Vacancy_Submitted_Event_Is_Handled(
        Guid id,
        IMessageHandlerContext context,
        List<NotificationEmail> notifications,
        [Frozen] Mock<INotificationService> notificationService,
        [Frozen] Mock<IQueueClient<NotificationEmail>> queueClient,
        [Greedy] OnVacancyEventHandler sut)
    {
        // arrange
        notificationService
            .Setup(x => x.CreateVacancyNotificationsAsync(id, VacancyStatus.Submitted, context.CancellationToken))
            .ReturnsAsync(notifications);

        // act
        await sut.Handle(new VacancySubmittedEvent(id), context);

        // assert
        notificationService.Verify(x => x.CreateVacancyNotificationsAsync(id, VacancyStatus.Submitted, context.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<NotificationEmail>(), context.CancellationToken), Times.Exactly(notifications.Count));
    }
}