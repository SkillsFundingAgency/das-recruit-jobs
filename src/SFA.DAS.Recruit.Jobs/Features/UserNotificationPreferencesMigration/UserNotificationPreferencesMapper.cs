using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using MongoUserNotificationPreferences = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.UserNotificationPreferences;
using MongoNotificationFrequency = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.NotificationFrequency;
using MongoNotificationScope = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.NotificationScope;
using MongoNotificationTypes = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.NotificationTypes;

namespace SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;

[ExcludeFromCodeCoverage]
public class UserNotificationPreferencesMapper(ILogger<UserNotificationPreferencesMapper> logger)
{
    public bool MapFrom(User user, MongoUserNotificationPreferences source)
    {
        try
        {
            var flags = Enum.GetValues<MongoNotificationTypes>().Cast<Enum>().Where(source.NotificationTypes.HasFlag).Cast<MongoNotificationTypes>();
            var scope = MapScope(source.NotificationScope);
            var frequency = MapFrequency(source.NotificationFrequency);
            var prefs = new NotificationPreferences
            {
                EventPreferences = flags.Select(x => x switch
                {
                    MongoNotificationTypes.None => null,
                    MongoNotificationTypes.VacancyRejected => new NotificationPreference(nameof(NotificationTypes.VacancyApprovedOrRejectedByDfE), "Email", scope, "Default"),
                    MongoNotificationTypes.VacancyClosingSoon => new NotificationPreference(nameof(NotificationTypes.VacancyClosingSoon), "Email", "Default", "Default"),
                    MongoNotificationTypes.ApplicationSubmitted => new NotificationPreference(nameof(NotificationTypes.ApplicationSubmitted), "Email", scope, frequency),
                    MongoNotificationTypes.VacancySentForReview => new NotificationPreference(nameof(NotificationTypes.VacancySentForReview), "Email", scope, "Default"),
                    MongoNotificationTypes.VacancyRejectedByEmployer => new NotificationPreference(nameof(NotificationTypes.VacancyRejectedByEmployer), "Email", scope, "Default"),
                    _ => null
                }).Where(x => x is not null).ToList()!
            };

            user.NotificationPreferences = JsonSerializer.Serialize(prefs);
            return true;
        }
        catch (ArgumentOutOfRangeException e)
        {
            logger.LogWarning("Failed to migrate '{UserNotificationPreferencesId}' due to an invalid enum value, source error: {sourceMessage}'", source.Id, e.Message);
            return false;
        }
    }

    private static string MapScope(MongoNotificationScope? source)
    {
        return source switch {
            MongoNotificationScope.UserSubmittedVacancies => nameof(NotificationScope.UserSubmittedVacancies),
            MongoNotificationScope.OrganisationVacancies => nameof(NotificationScope.OrganisationVacancies),
            null => "Default",
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }

    private static string MapFrequency(MongoNotificationFrequency? source)
    {
        return source switch {
            MongoNotificationFrequency.Immediately => nameof(NotificationFrequency.Immediately),
            MongoNotificationFrequency.Daily => nameof(NotificationFrequency.Daily),
            MongoNotificationFrequency.Weekly => nameof(NotificationFrequency.Weekly),
            null => "Default",
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}