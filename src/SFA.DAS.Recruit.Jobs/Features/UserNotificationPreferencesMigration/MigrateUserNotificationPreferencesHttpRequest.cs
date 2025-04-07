using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;

[ExcludeFromCodeCoverage]
public class MigrateUserNotificationPreferencesHttpRequest
{
    [JsonPropertyName("userNotificationPreferencesIds")]
    public required List<string> Ids { get; init; }
}