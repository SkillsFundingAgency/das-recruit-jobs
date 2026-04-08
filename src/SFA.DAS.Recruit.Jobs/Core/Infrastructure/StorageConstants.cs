namespace SFA.DAS.Recruit.Jobs.Core.Infrastructure;

public static class StorageConstants
{
    public static class QueueNames
    {
        public const string AiVacancyReviewRequests = "ai-vacancy-review-requests";
        public const string DelayedNotifications = "delayed-notifications";
        public const string Notifications = "recruit-jobs-pending-notifications";
    }
}