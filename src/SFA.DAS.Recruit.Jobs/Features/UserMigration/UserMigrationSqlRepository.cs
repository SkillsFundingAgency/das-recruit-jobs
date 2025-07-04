using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.UserMigration;

[ExcludeFromCodeCoverage]
public class UserMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertUsersBatchAsync(List<User> users)
    {
        await dataContext.BulkInsertOrUpdateAsync(users);
    }
}