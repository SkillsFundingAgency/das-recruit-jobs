namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

public class UserNotificationPreferences
{
    public string Id { get; set; }
    public string DfeUserId { get; set; }
    public NotificationTypes NotificationTypes { get; set; }
    public NotificationFrequency? NotificationFrequency { get; set; }
    public NotificationScope? NotificationScope { get; set; }
    public DateTime? MigrationDate { get; set; }
    public bool? MigrationIgnore { get; set; }
}

[Flags]
public enum NotificationTypes
{
    None = 0,
    VacancyRejected = 1,
    VacancyClosingSoon = 1 << 1,
    ApplicationSubmitted = 1 << 2,
    VacancySentForReview = 1 << 3,
    VacancyRejectedByEmployer = 1 << 4
}

public enum NotificationScope
{
    UserSubmittedVacancies,
    OrganisationVacancies
}

public enum NotificationFrequency
{
    Immediately,
    Daily,
    Weekly
}