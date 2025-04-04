using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using MongoUserNotificationPreferences = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.UserNotificationPreferences;

namespace SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;

[ExcludeFromCodeCoverage]
public class UserNotificationPreferencesMigrationStrategy(
    ILogger<UserNotificationPreferencesMigrationStrategy> logger,
    UserNotificationPreferencesMigrationMongoRepository mongoRepository,
    UserNotificationPreferencesMigrationSqlRepository sqlRepository)
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
        var userNotificationPreferences = await mongoRepository.FetchBatchAsync(BatchSize);
        while (userNotificationPreferences is { Count: > 0 } && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            await ProcessBatch(userNotificationPreferences);
            userNotificationPreferences = await mongoRepository.FetchBatchAsync(BatchSize);
        }
    }
    
    private async Task ProcessBatch(List<MongoUserNotificationPreferences> userNotificationPreferences)
    {
        logger.LogInformation("Started processing a batch of {count} records", userNotificationPreferences.Count);
        
        // Process the records
        var toMap = userNotificationPreferences.Where(x => Guid.TryParse(x.Id, out _)).ToList();
        var toIgnore = userNotificationPreferences.Except(toMap).ToList();

        if (toMap is { Count: > 0 })
        {
            var mappedRecords = toMap.Select(UserNotificationPreferencesMapper.MapFrom).ToList();
            
            // Push the data to SQL server
            await sqlRepository.UpsertApplicationReviewsBatchAsync(mappedRecords);
            logger.LogInformation("Imported {count} user notification preferences", mappedRecords.Count);

            // Mark migrated in Mongo
            await mongoRepository.UpdateSuccessMigrationDateBatchAsync(toMap.Select(x => x.Id).ToList());
            logger.LogInformation("Marked {SuccessCount} user notification preferences as migrated", mappedRecords.Count);
        }

        if (toIgnore is { Count: > 0 })
        {
            // Mark 'ignore' in Mongo
            await mongoRepository.UpdateFailedMigrationDateBatchAsync(toIgnore.Select(x => x.Id).ToList());
            logger.LogInformation("Marked {IgnoreCount} user notification preferences as 'ignore'", toIgnore.Count);
        }
    }
}