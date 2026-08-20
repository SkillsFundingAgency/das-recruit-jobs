using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public class PostCreateVacancyFeedbackNudgeNotifications(object data): IPostRequest
{
    public string Url => "notifications/create/vacancy-feedback-nudge";
    public object? Data => data;
}