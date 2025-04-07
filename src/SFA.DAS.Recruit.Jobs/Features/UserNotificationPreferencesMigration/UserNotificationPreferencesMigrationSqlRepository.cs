using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;

[ExcludeFromCodeCoverage]
public class UserNotificationPreferencesMigrationSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertApplicationReviewsBatchAsync(List<UserNotificationPreferences> userNotificationPreferences)
    {
        await dataContext.BulkInsertOrUpdateAsync(userNotificationPreferences);
    }
}