using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Features.UserMigration;
using MongoUserNotificationPreferences = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.UserNotificationPreferences;

namespace SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;

[ExcludeFromCodeCoverage]
public class UserNotificationPreferencesMigrationStrategy(
    ILogger<UserNotificationPreferencesMigrationStrategy> logger,
    UserNotificationPreferencesMigrationMongoRepository mongoRepository,
    UserMigrationSqlRepository userRepository,
    UserNotificationPreferencesMapper mapper)
{
    private const int BatchSize = 500;
    private const int MaxRuntimeInSeconds = 270; // 4m 30s

    public async Task RunAsync(List<string> ids)
    {
        var userNotificationPreferences = await mongoRepository.FetchBatchByIdsAsync(ids);
        await ProcessBatch(userNotificationPreferences);
    }

    public async Task RunAsync()
    {
        var startTime = DateTime.UtcNow;
        var remigrateIfBeforeDate = new DateTime(2025, 08, 21); // set to a date after a migration to trigger reimport
        var userNotificationPreferences = await mongoRepository.FetchBatchAsync(BatchSize, remigrateIfBeforeDate);
        while (userNotificationPreferences is { Count: > 0 } && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            await ProcessBatch(userNotificationPreferences);
            userNotificationPreferences = await mongoRepository.FetchBatchAsync(BatchSize, remigrateIfBeforeDate);
        }
    }

    private async Task ProcessBatch(List<MongoUserNotificationPreferences> userNotificationPreferences)
    {
        logger.LogInformation("Started processing a batch of {count} records", userNotificationPreferences.Count);
        
        // Process the records
        List<MongoUserNotificationPreferences> excluded = [];
        List<string> mappedIds = [];
        foreach (var userNotificationPreference in userNotificationPreferences)
        {
            var user = await userRepository.FindUserByIdAsync(userNotificationPreference.Id);
            if (user is null)
            {
                logger.LogWarning("Could not migrate record '{UserNotificationPreferenceId}' - could not locate a matching user", userNotificationPreference.Id);
                excluded.Add(userNotificationPreference);
                continue;
            }

            if (mapper.MapFrom(user, userNotificationPreference))
            {
                mappedIds.Add(userNotificationPreference.Id);
            }
            else
            {
                excluded.Add(userNotificationPreference);
            }
        }
        
        // Push the data to SQL server
        await userRepository.SaveChangesAsync();
        logger.LogInformation("Imported {count} user notification preferences", mappedIds.Count);

        // Mark migrated in Mongo
        await mongoRepository.UpdateSuccessMigrationDateBatchAsync(mappedIds);
        logger.LogInformation("Marked {SuccessCount} user notification preferences as migrated", mappedIds.Count);

        if (excluded is { Count: > 0 })
        {
            // Mark 'ignore' in Mongo
            await mongoRepository.UpdateFailedMigrationDateBatchAsync(excluded.Select(x => x.Id).ToList());
            logger.LogInformation("Marked {IgnoreCount} user notification preferences as 'ignore'", excluded.Count);
        }
    }
}