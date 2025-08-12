namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class NotificationPreferences
{
    public List<NotificationPreference> EventPreferences { get; set; } = [];
}

public record NotificationPreference(string Event, string Method, string Scope, string Frequency);