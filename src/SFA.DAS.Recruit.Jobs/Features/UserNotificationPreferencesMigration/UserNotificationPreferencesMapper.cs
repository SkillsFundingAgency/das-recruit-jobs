﻿using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using MongoUserNotificationPreferences = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.UserNotificationPreferences;
using MongoNotificationFrequency = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.NotificationFrequency;
using MongoNotificationScope = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.NotificationScope;
using MongoNotificationTypes = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.NotificationTypes;

namespace SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;

[ExcludeFromCodeCoverage]
public class UserNotificationPreferencesMapper
{
    public static UserNotificationPreferences MapFrom(MongoUserNotificationPreferences source)
    {
        return new UserNotificationPreferences
        {
            UserId = Guid.Parse(source.Id),
            Frequency = MapFrequency(source.NotificationFrequency),
            Scope = MapScope(source.NotificationScope),
            Types = MapTypes(source.NotificationTypes),
        };
    }

    private static NotificationTypes? MapTypes(MongoNotificationTypes? source)
    {
        if (source is null) return null;
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