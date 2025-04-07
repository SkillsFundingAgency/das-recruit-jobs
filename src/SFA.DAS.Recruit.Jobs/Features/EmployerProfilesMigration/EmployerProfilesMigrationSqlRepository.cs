using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.EmployerProfilesMigration;

[ExcludeFromCodeCoverage]
public class EmployerProfilesMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertEmployerProfilesBatchAsync(List<EmployerProfile> employerProfiles)
    {
        await dataContext.BulkInsertOrUpdateAsync(employerProfiles);
    }

    public async Task UpsertEmployerProfileAddressesBatchAsync(List<EmployerProfileAddress> mappedAddresses)
    {
        var config = new BulkConfig
        {
            UpdateByProperties = [
                nameof(EmployerProfileAddress.AccountLegalEntityId),
                nameof(EmployerProfileAddress.AddressLine1),
                nameof(EmployerProfileAddress.Postcode)
            ],
            PropertiesToExclude = [
                nameof(EmployerProfileAddress.Id)
            ]
        };
        await dataContext.BulkInsertOrUpdateAsync(mappedAddresses, config);
    }
}