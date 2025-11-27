using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using SqlBlockedOrganisation = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.BlockedOrganisation;
using MongoBlockedOrganisation = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.BlockedOrganisation;

namespace SFA.DAS.Recruit.Jobs.Features.BlockedOrganisationsMigration;

[ExcludeFromCodeCoverage]
public class BlockedOrganisationMapper
{
    public SqlBlockedOrganisation MapFrom(MongoBlockedOrganisation blockedOrganisation)
    {
        return new SqlBlockedOrganisation
        {
            Id = blockedOrganisation.Id,
            BlockedStatus = blockedOrganisation.BlockedStatus switch
            {
                BlockedStatus.Blocked => DataAccess.Sql.Domain.BlockedStatus.Blocked,
                BlockedStatus.Unblocked => DataAccess.Sql.Domain.BlockedStatus.Unblocked,
            },
            OrganisationType = blockedOrganisation.OrganisationType switch
            {
                OrganisationType.Employer => DataAccess.Sql.Domain.OrganisationType.Employer,
                OrganisationType.Provider => DataAccess.Sql.Domain.OrganisationType.Provider,
            },
            OrganisationId = blockedOrganisation.OrganisationId,
            Reason = blockedOrganisation.Reason ?? "Unknown",
            UpdatedByUserId = blockedOrganisation.UpdatedByUser?.Name ?? "Unknown",
            UpdatedByUserEmail = blockedOrganisation.UpdatedByUser?.Email ?? "Unknown",
            UpdatedDate = blockedOrganisation.UpdatedDate,
        };
    }
}