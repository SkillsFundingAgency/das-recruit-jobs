using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Handlers;

public interface IDelayedNotificationsDeliveryHandler
{
    Task RunAsync(QueueItem<NotificationEmail> item, CancellationToken cancellationToken);
}

public class DelayedNotificationsDeliveryHandler(IRecruitJobsOuterClient jobsOuterClient): IDelayedNotificationsDeliveryHandler
{
    public async Task RunAsync(QueueItem<NotificationEmail> item, CancellationToken cancellationToken)
    {
        await jobsOuterClient.SendEmailAsync(item.Payload, cancellationToken);
    }
}