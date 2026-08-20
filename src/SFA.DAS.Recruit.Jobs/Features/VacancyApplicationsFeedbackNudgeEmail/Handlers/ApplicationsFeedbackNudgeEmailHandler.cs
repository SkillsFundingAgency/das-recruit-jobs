using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyApplicationsFeedbackNudgeEmail.Handlers;

public interface IApplicationsFeedbackNudgeEmailHandler
{
    Task RunAsync(CancellationToken cancellationToken);
}

public class ApplicationsFeedbackNudgeEmailHandler(
    IJobsOuterClient jobsOuterClient,
    IQueueClient<NotificationEmail> queueClient): IApplicationsFeedbackNudgeEmailHandler
{
    private const int DaysValue = -29; // 4 weeks from yesterday
    
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var date = DateTime.UtcNow.AddDays(DaysValue);
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

    private async Task<List<VacancyApplicationsCountRequiringFeedback>> GetVacanciesInfoAsync(DateTime date, CancellationToken cancellationToken)
    {
        var request = new GetVacanciesWithApplicationsNeedingFeedbackForDate(date);
        var response = await jobsOuterClient.GetAsync<List<VacancyApplicationsCountRequiringFeedback>>(request, cancellationToken);
        response.ThrowIfErrored();
        return response.Payload ?? [];
    }
}