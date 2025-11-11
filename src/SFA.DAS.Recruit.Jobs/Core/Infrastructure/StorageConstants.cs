namespace SFA.DAS.Recruit.Jobs.Core.Infrastructure;

public static class StorageConstants
{
    public static class QueueNames
    {
        public const string DelayedNotifications = "delayed-notifications";
        public const string TransferVacanciesFromProviderQueueName = "transfer-vacancies-from-provider-queue-v2";
        public const string TransferVacancyToLegalEntityQueueName = "transfer-vacancies-to-legal-entity-queue-v2";
    }
}