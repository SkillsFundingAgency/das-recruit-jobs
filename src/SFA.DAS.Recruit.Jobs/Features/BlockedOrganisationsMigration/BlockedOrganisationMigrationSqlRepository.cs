using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.BlockedOrganisationsMigration;

[ExcludeFromCodeCoverage]
public class BlockedOrganisationMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    private static readonly BulkConfig Config = new() { UseTempDB = true };
    
    public async Task UpsertBlockedOrganisationsBatchAsync(List<BlockedOrganisation> blockedOrganisations)
    {
        await dataContext.BulkInsertOrUpdateAsync(blockedOrganisations, Config);
    }
}