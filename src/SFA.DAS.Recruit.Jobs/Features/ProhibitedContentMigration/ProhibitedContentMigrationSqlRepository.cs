using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.ProhibitedContentMigration;

[ExcludeFromCodeCoverage]
public class ProhibitedContentMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertProhibitedContentBatchAsync(List<ProhibitedContent> prohibitedContents)
    {
        await dataContext.BulkInsertOrUpdateAsync(prohibitedContents);
    }
}