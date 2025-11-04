using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Handlers;

public interface IDelayedNotificationsDeliveryHandler
{
    Task RunAsync(NotificationEmail notificationEmail, CancellationToken cancellationToken);
}

public class DelayedNotificationsDeliveryHandler(IRecruitJobsOuterClient jobsOuterClient): IDelayedNotificationsDeliveryHandler
{
    public async Task RunAsync(NotificationEmail notificationEmail, CancellationToken cancellationToken)
    {
        await jobsOuterClient.SendEmailAsync(notificationEmail, cancellationToken);
    }
}