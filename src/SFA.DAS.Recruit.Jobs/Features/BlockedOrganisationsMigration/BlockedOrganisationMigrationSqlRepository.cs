using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.BlockedOrganisationsMigration;

[ExcludeFromCodeCoverage]
public class BlockedOrganisationMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    private static readonly BulkConfig Config = new() { UseTempDB = true };
    
    public async Task UpsertBlockedOrganisationsBatchAsync(List<BlockedOrganisation> blockedOrganisations)
    {
        var strategy = dataContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dataContext.Database.BeginTransactionAsync();
            await dataContext.BulkInsertOrUpdateAsync(blockedOrganisations, Config);
            await transaction.CommitAsync();
        });
    }
}