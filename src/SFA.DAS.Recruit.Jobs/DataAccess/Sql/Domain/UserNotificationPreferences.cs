namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class UserNotificationPreferences
{
    public required Guid UserId { get; init; }
    public NotificationTypes? Types { get; init; }
    public NotificationFrequency? Frequency { get; init; }
    public NotificationScope? Scope { get; init; }

    public static readonly UserNotificationPreferences None = new() { UserId = Guid.Empty };
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