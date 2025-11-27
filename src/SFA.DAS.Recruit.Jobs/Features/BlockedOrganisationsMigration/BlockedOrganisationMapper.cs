using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using SFA.DAS.Recruit.Jobs.Features.VacancyMigration;
using SqlBlockedOrganisation = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.BlockedOrganisation;
using MongoBlockedOrganisation = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.BlockedOrganisation;

namespace SFA.DAS.Recruit.Jobs.Features.BlockedOrganisationsMigration;

[ExcludeFromCodeCoverage]
public class BlockedOrganisationMapper(ILogger<BlockedOrganisationMapper> logger, UserLocator userLocator)
{
    public async Task<SqlBlockedOrganisation> MapFromAsync(MongoBlockedOrganisation blockedOrganisation)
    {
        var updatedByUserId = await userLocator.LocateMongoUserAsync(blockedOrganisation.UpdatedByUser);
        if (updatedByUserId is null)
        {
            logger.LogWarning("Failed to find user who updated the blocked organisation for record '{BlockedOrganisationId}'", blockedOrganisation.Id);
            return SqlBlockedOrganisation.None;
        }
        
        return new SqlBlockedOrganisation
        {
            Id = blockedOrganisation.Id,
            BlockedStatus = blockedOrganisation.BlockedStatus switch
            {
                DataAccess.MongoDb.Domain.BlockedStatus.Blocked => BlockedStatus.Blocked,
                DataAccess.MongoDb.Domain.BlockedStatus.Unblocked => BlockedStatus.Unblocked,
            },
            OrganisationType = blockedOrganisation.OrganisationType switch
            {
                DataAccess.MongoDb.Domain.OrganisationType.Employer => OrganisationType.Employer,
                DataAccess.MongoDb.Domain.OrganisationType.Provider => OrganisationType.Provider,
            },
            OrganisationId = blockedOrganisation.OrganisationId,
            Reason = blockedOrganisation.Reason ?? "Unknown",
            UpdatedByUserId = updatedByUserId.Value,
            UpdatedDate = blockedOrganisation.UpdatedDate,
        };
    }
}