using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public readonly struct PostSendNotificationRequest(NotificationEmail notification): IPostRequest
{
    public string Url => "notifications/send";
    public object? Data => notification;
}