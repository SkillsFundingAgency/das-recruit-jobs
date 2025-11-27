using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using MongoBlockedOrganisation = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.BlockedOrganisation;
using SqlBlockedOrganisation = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.BlockedOrganisation;

namespace SFA.DAS.Recruit.Jobs.Features.BlockedOrganisationsMigration;

[ExcludeFromCodeCoverage]
public class BlockedOrganisationMigrationStrategy(
    ILogger<BlockedOrganisationMigrationStrategy> logger,
    BlockedOrganisationMapper mapper,
    BlockedOrganisationMigrationMongoRepository mongoRepository,
    BlockedOrganisationMigrationSqlRepository sqlRepository)
{
    private const int BatchSize = 200;
    private const int MaxRuntimeInSeconds = 270; // 4m 30s
    
    public async Task RunAsync()
    {
        var startTime = DateTime.UtcNow;
        var remigrateIfBeforeDate = new DateTime(2025, 01, 01); // set to a date after a migration to trigger reimport
        var mongoBlockedOrganisations = await mongoRepository.FetchBatchAsync(BatchSize, remigrateIfBeforeDate);
        while (mongoBlockedOrganisations is { Count: > 0 } && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            await ProcessBatchAsync(mongoBlockedOrganisations);
            mongoBlockedOrganisations = await mongoRepository.FetchBatchAsync(BatchSize, remigrateIfBeforeDate);
        }
    }

    private async Task ProcessBatchAsync(List<MongoBlockedOrganisation> blockedOrganisations)
    {
        List<SqlBlockedOrganisation> mappedBlockedOrganisations = [];
        mappedBlockedOrganisations.AddRange(blockedOrganisations.Select(mapper.MapFrom));
        
        if (mappedBlockedOrganisations is { Count: > 0 })
        {
            await sqlRepository.UpsertBlockedOrganisationsBatchAsync(mappedBlockedOrganisations);
            logger.LogInformation("Imported {count} BlockedOrganisations", mappedBlockedOrganisations.Count);
                
            await mongoRepository.UpdateSuccessMigrationDateBatchAsync(mappedBlockedOrganisations.Select(x => x.Id).ToList());
            logger.LogInformation("Marked {SuccessCount} BlockedOrganisations as migrated", mappedBlockedOrganisations.Count);
        }
    }
}