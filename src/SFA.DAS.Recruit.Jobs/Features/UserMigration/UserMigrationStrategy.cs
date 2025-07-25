using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using MongoUser = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.User;
using SqlUser = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.User;

namespace SFA.DAS.Recruit.Jobs.Features.UserMigration;

[ExcludeFromCodeCoverage]
public class UserMigrationStrategy(
    ILogger<UserMigrationStrategy> logger,
    UserMapper mapper,
    UserMigrationMongoRepository mongoRepository,
    UserMigrationSqlRepository sqlRepository)
{
    private const int BatchSize = 200;
    private const int MaxRuntimeInSeconds = 270; // 4m 30s
    
    public async Task RunAsync()
    {
        var startTime = DateTime.UtcNow;
        var remigrateIfBeforeDate = new DateTime(2025, 01, 01); // set to a date after a migration to trigger reimport
        var mongoUsers = await mongoRepository.FetchBatchAsync(BatchSize, remigrateIfBeforeDate);
        while (mongoUsers is { Count: > 0 } && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            await ProcessBatchAsync(mongoUsers);
            mongoUsers = await mongoRepository.FetchBatchAsync(BatchSize, remigrateIfBeforeDate);
        }
    }

    private async Task ProcessBatchAsync(List<MongoUser> users)
    {
        List<MongoUser> excluded = [];
        List<SqlUser> mappedUsers = [];
        foreach (var user in users)
        {
            var item = mapper.MapFrom(user);
            if (item == SqlUser.None)
            {
                excluded.Add(user);
            }
            else
            {
                mappedUsers.Add(item);
            }
        }
        
        if (excluded is { Count: > 0 })
        {
            await mongoRepository.UpdateFailedMigrationDateBatchAsync(excluded.Select(x => x.Id).ToList());
            logger.LogInformation("Failed to migrate {FailedCount} Users", excluded.Count);
        }

        if (mappedUsers is { Count: > 0 })
        {
            await sqlRepository.UpsertUsersBatchAsync(mappedUsers);
            logger.LogInformation("Imported {count} Users", mappedUsers.Count);
                
            await mongoRepository.UpdateSuccessMigrationDateBatchAsync(mappedUsers.Select(x => x.Id).ToList());
            logger.LogInformation("Marked {SuccessCount} Users as migrated", mappedUsers.Count);
        }
    }
}