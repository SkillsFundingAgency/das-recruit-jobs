using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using SFA.DAS.Recruit.Jobs.Features.VacancyMigration;
using MongoUserNotificationPreferences = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.UserNotificationPreferences;
using MongoNotificationFrequency = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.NotificationFrequency;
using MongoNotificationScope = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.NotificationScope;
using MongoNotificationTypes = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.NotificationTypes;

namespace SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;

[ExcludeFromCodeCoverage]
public class UserNotificationPreferencesMapper(ILogger<UserNotificationPreferencesMapper> logger, UserLocator userLocator)
{
    public async Task<UserNotificationPreferences> MapFromAsync(MongoUserNotificationPreferences source)
    {
        var userId = await userLocator.LocateAsync(source.Id);
        if (userId is null)
        {
            logger.LogWarning("Failed to migrate '{UserNotificationPreferencesId}', could not locate User", source.Id);
            return UserNotificationPreferences.None;
        }
        
        try
        {
            return new UserNotificationPreferences
            {
                UserId = Guid.Parse(source.Id),
                Frequency = MapFrequency(source.NotificationFrequency),
                Scope = MapScope(source.NotificationScope),
                Types = MapTypes(source.NotificationTypes),
            };
        }
        catch (ArgumentOutOfRangeException e)
        {
            logger.LogWarning("Failed to migrate '{UserNotificationPreferencesId}' due to an invalid enum value, source error: {sourceMessage}'", source.Id, e.Message);
            return UserNotificationPreferences.None;
        }
    }

    private static NotificationTypes? MapTypes(MongoNotificationTypes? source)
    {
        if (source is null) return null;
        if ((source & MongoNotificationTypes.VacancySubmittedForReview) > 0)
        {
            throw new ArgumentOutOfRangeException("NotificationType", "Enum value 'NotificationTypes.VacancySubmittedForReview' is no longer supported.");
        }
        var value = (int)source;
        return (NotificationTypes)value;
    }

    private static NotificationScope? MapScope(MongoNotificationScope? source)
    {
        return source switch {
            MongoNotificationScope.UserSubmittedVacancies => NotificationScope.UserSubmittedVacancies,
            MongoNotificationScope.OrganisationVacancies => NotificationScope.OrganisationVacancies,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }

    private static NotificationFrequency? MapFrequency(MongoNotificationFrequency? source)
    {
        return source switch {
            MongoNotificationFrequency.Immediately => NotificationFrequency.Immediately,
            MongoNotificationFrequency.Daily => NotificationFrequency.Daily,
            MongoNotificationFrequency.Weekly => NotificationFrequency.Weekly,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}