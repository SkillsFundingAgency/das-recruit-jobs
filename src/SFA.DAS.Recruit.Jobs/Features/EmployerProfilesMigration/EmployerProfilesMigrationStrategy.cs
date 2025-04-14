using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;
using SqlEmployerProfileAddress = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.EmployerProfileAddress;
using SqlEmployerProfile = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.EmployerProfile;
using MongoEmployerProfile = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.EmployerProfile;
using MongoAddress = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.Address;

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
            await sqlRepository.UpsertEmployerProfileAddressesBatchAsync(mappedAddresses);
            
            // Mark migrated
            await mongoRepository.UpdateSuccessMigrationDateBatchAsync(migratedRecords.Select(x => x.Id).ToList());
        }

        if (excludedEmployerProfiles is { Count: > 0 })
        {
            // Mark failed        
            await mongoRepository.UpdateFailedMigrationDateBatchAsync(excludedEmployerProfiles.Select(x => x.Id).ToList());
        }
    }
}