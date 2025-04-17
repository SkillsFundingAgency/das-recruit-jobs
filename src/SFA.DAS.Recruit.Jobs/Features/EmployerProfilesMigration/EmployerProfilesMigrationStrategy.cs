using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SqlEmployerProfileAddress = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.EmployerProfileAddress;
using SqlEmployerProfile = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.EmployerProfile;
using MongoEmployerProfile = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.EmployerProfile;

namespace SFA.DAS.Recruit.Jobs.Features.EmployerProfilesMigration;

[ExcludeFromCodeCoverage]
public class EmployerProfilesMigrationStrategy(
    ILogger<EmployerProfilesMigrationStrategy> logger,
    EmployerProfilesMigrationMongoRepository mongoRepository,
    EmployerProfilesMigrationSqlRepository sqlRepository,
    EmployerProfilesMapper mapper)
{
    private const int BatchSize = 100;
    private const int MaxRuntimeInSeconds = 270; // 4m 30s
    
    public async Task RunAsync()
    {
        var startTime = DateTime.UtcNow;
        var employerProfiles = await mongoRepository.FetchBatchAsync(BatchSize);
        while (employerProfiles is { Count: > 0 } && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            await ProcessBatchAsync(employerProfiles);
            employerProfiles = await mongoRepository.FetchBatchAsync(BatchSize);
        }
    }

    private async Task ProcessBatchAsync(List<MongoEmployerProfile> employerProfiles)
    {
        // Map to new records
        List<MongoEmployerProfile> excludedEmployerProfiles = [];
        var mappedProfiles = employerProfiles
            .Select(x =>
            {
                var item = mapper.MapProfileFrom(x);
                if (item == SqlEmployerProfile.None)
                {
                    excludedEmployerProfiles.Add(x);
                }

                return item;
            })
            .Where(x => x != SqlEmployerProfile.None)
            .ToList();

        var migratedRecords = employerProfiles.Except(excludedEmployerProfiles).ToList();

        // Only get addresses for successful mappings
        List<SqlEmployerProfileAddress> mappedAddresses = [];
        if (migratedRecords is { Count: > 1 })
        {
            mappedAddresses.AddRange(mapper.MapAddressesFrom(migratedRecords));
        }

        if (mappedProfiles is { Count: > 0 })
        {
            // Push to SQL Server
            await sqlRepository.UpsertEmployerProfilesBatchAsync(mappedProfiles);
            logger.LogInformation("Imported {count} employer profiles", mappedProfiles.Count);

            if (mappedAddresses is { Count: > 0 })
            {
                await sqlRepository.UpsertEmployerProfileAddressesBatchAsync(mappedAddresses);
                logger.LogInformation("Imported {count} employer profile addresses", mappedAddresses.Count);
            }
            
            // Mark migrated
            await mongoRepository.UpdateSuccessMigrationDateBatchAsync(migratedRecords.Select(x => x.Id).ToList());
            logger.LogInformation("Marked {SuccessCount} employer profiles as migrated", migratedRecords.Count);
        }

        if (excludedEmployerProfiles is { Count: > 0 })
        {
            // Mark failed        
            await mongoRepository.UpdateFailedMigrationDateBatchAsync(excludedEmployerProfiles.Select(x => x.Id).ToList());
            logger.LogInformation("Failed to migrate {FailedCount} employer profiles", excludedEmployerProfiles.Count);
        }
    }
}