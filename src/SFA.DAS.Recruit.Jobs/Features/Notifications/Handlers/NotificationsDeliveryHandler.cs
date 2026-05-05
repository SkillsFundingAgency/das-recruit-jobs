using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.Features.Notifications.Handlers;

public interface INotificationsDeliveryHandler
{
    Task RunAsync(QueueItem<NotificationEmail> item, CancellationToken cancellationToken);
}

public class NotificationsDeliveryHandler(IJobsOuterClient jobsOuterClient): INotificationsDeliveryHandler
{
    public async Task RunAsync(QueueItem<NotificationEmail> item, CancellationToken cancellationToken)
    {
        await jobsOuterClient.PostAsync(new PostSendNotificationRequest(item.Payload), cancellationToken);
    }
}