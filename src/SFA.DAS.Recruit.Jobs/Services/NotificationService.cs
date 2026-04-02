using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.Services;

public interface INotificationService
{
    Task<List<NotificationEmail>> CreateVacancyNotificationsAsync(Guid id, CancellationToken cancellationToken);
    Task SendNotificationAsync(NotificationEmail notification, CancellationToken cancellationToken);
}

public class NotificationService(IJobsOuterClient jobsOuterClient): INotificationService
{
    public async Task<List<NotificationEmail>> CreateVacancyNotificationsAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await jobsOuterClient.PostAsync<DataResponse<List<NotificationEmail>>>(new PostCreateVacancyNotificationsRequest(id), cancellationToken);
        response.ThrowIfErrored($"Failed to generate notifications for vacancy '{id}'");
        return response.Payload!.Data;
    }

    public async Task SendNotificationAsync(NotificationEmail notification, CancellationToken cancellationToken)
    {
        var response = await jobsOuterClient.PostAsync(new PostSendNotificationRequest(notification), cancellationToken);
        response.ThrowIfErrored("Failed to send notification");
    }
}