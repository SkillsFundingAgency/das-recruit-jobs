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
        var mappedProfiles = employerProfiles.Select(mapper.MapProfileFrom).ToList();
        var mappedAddresses = mapper.MapAddressesFrom(employerProfiles).ToList();

        await sqlRepository.UpsertEmployerProfilesBatchAsync(mappedProfiles);
        await sqlRepository.UpsertEmployerProfileAddressesBatchAsync(mappedAddresses);
        
        await mongoRepository.UpdateSuccessMigrationDateBatchAsync(employerProfiles.Select(x => x.Id).ToList());
    }
}