using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.BlockedOrganisationsMigration;

[ExcludeFromCodeCoverage]
public class BlockedOrganisationMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertBlockedOrgsBatchAsync(List<BlockedOrganisation> blockedOrganisations)
    {
        await dataContext.BulkInsertOrUpdateAsync(blockedOrganisations);
    }
}