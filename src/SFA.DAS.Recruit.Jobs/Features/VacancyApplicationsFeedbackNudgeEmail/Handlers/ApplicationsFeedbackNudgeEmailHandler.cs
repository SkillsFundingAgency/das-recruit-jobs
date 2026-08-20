using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyApplicationsFeedbackNudgeEmail.Handlers;

public interface IApplicationsFeedbackNudgeEmailHandler
{
    Task RunAsync(DateOnly date, CancellationToken cancellationToken);
}

public class ApplicationsFeedbackNudgeEmailHandler(
    IJobsOuterClient jobsOuterClient,
    IQueueClient<NotificationEmail> queueClient): IApplicationsFeedbackNudgeEmailHandler
{
    public async Task RunAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var vacancyInfos = await GetVacanciesInfoAsync(date, cancellationToken);
        if (vacancyInfos is not { Count: > 0 })
        {
            return;
        }
        
        var notifications = await GetNotificationsAsync(vacancyInfos, cancellationToken);
        foreach (var notification in notifications)
        {
            await queueClient.SendMessageAsync(notification, cancellationToken);
        }   
    }

    private async Task<List<NotificationEmail>> GetNotificationsAsync(List<VacancyApplicationsCountRequiringFeedback> vacancyInfos, CancellationToken cancellationToken)
    {
        var request = new PostCreateVacancyFeedbackNudgeNotifications(vacancyInfos);
        var response = await jobsOuterClient.PostAsync<DataResponse<List<NotificationEmail>>>(request, cancellationToken);
        response.ThrowIfErrored();
        return response.Payload?.Data ?? [];
    }

    private async Task<List<VacancyApplicationsCountRequiringFeedback>> GetVacanciesInfoAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var request = new GetVacanciesWithApplicationsNeedingFeedbackForDate(date.ToDateTime(TimeOnly.MinValue));
        var response = await jobsOuterClient.GetAsync<List<VacancyApplicationsCountRequiringFeedback>>(request, cancellationToken);
        response.ThrowIfErrored();
        return response.Payload ?? [];
    }
}