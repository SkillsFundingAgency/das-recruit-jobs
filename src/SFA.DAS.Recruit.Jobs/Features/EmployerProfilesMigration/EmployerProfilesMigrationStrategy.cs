using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using MongoEmployerProfile = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.EmployerProfile;
using SqlEmployerProfile = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.EmployerProfile;
using SqlEmployerProfileAddress = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.EmployerProfileAddress;

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
            var records = mapper
                .MapAddressesFrom(migratedRecords)
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();
            mappedAddresses.AddRange(records.DistinctBy(x => $"{x.AccountLegalEntityId}-{x.AddressLine1}-{x.Postcode}"));

            if (records.Count != mappedAddresses.Count)
            {
                var groupedRecords = records.GroupBy(x => $"{x.AccountLegalEntityId}-{x.AddressLine1}-{x.Postcode}").Where(x => x.Count() > 1);
                var ids = string.Join(",", groupedRecords.Select(x => x.First().AccountLegalEntityId));
                logger.LogWarning("Detected duplicate addresses for the following Employer Profiles: {accountLegalEntityIds}", ids);
            }
        }

        if (mappedProfiles is { Count: > 0 })
        {
            // Push to SQL Server
            await sqlRepository.UpsertEmployerProfilesBatchAsync(mappedProfiles);
            logger.LogInformation("Imported {count} employer profiles", mappedProfiles.Count);

            if (mappedAddresses is { Count: > 0 })
            {
                // de-dupe addresses as we can't match on source ids that don't exist
                mappedAddresses = mappedAddresses.DistinctBy(x => $"{x.AccountLegalEntityId}-{x.AddressLine1}-{x.Postcode}").ToList();
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