using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
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

    public async Task<User?> FindUserByIdAndEmailAsync(string searchTerm, string? email)
    {
        return await dataContext.User.Where(x => (x.IdamsUserId == searchTerm || x.DfEUserId == searchTerm || x.Id.ToString() == searchTerm) && x.Email == email).FirstOrDefaultAsync();
    }
    
    public async Task<User?> FindUserByIdAsync(string searchTerm)
    {
        return await dataContext.User.Where(x => x.IdamsUserId == searchTerm || x.DfEUserId == searchTerm || x.Id.ToString() == searchTerm).FirstOrDefaultAsync();
    }

    public async Task<List<User>> FindUsersByEmailAsync(string? email)
    {
        return await dataContext.User.Where(x => x.Email == email).ToListAsync();
    }
}