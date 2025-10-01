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
        var newUsers = new List<User>();
        foreach (var user in users)
        {
            var existingUser = await dataContext
                .User
                .Include(x => x.EmployerAccounts)
                .FirstOrDefaultAsync(x => x.Id == user.Id);
            
            if (existingUser == null)
            {
                newUsers.Add(user);
                continue;
            }

            // save these
            var notifications = existingUser.NotificationPreferences;
            
            dataContext.Entry(existingUser).CurrentValues.SetValues(user);
            
            // restore them as they'll get wiped otherwise
            existingUser.NotificationPreferences = notifications;
            
            // remove the deleted employerAccountIds
            var newEntityIds = user.EmployerAccounts == null ? [] : user.EmployerAccounts.Select(x => x.EmployerAccountId).ToList();

            existingUser.EmployerAccounts?.RemoveAll(x => !newEntityIds.Contains(x.EmployerAccountId));


            // add new employerAccountIds
            var oldEntityIds = existingUser.EmployerAccounts == null ? [] : existingUser.EmployerAccounts.Select(x => x.EmployerAccountId).ToList();
            var newEmployerAccounts = user.EmployerAccounts == null ? [] : user.EmployerAccounts.Where(x => !oldEntityIds.Contains(x.EmployerAccountId)).ToList();

            existingUser.EmployerAccounts ??= [];
            existingUser.EmployerAccounts.AddRange(newEmployerAccounts);
        }
        
        dataContext.AddRange(newUsers);
        await dataContext.SaveChangesAsync();
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

    public async Task SaveChangesAsync()
    {
        await dataContext.SaveChangesAsync();
    }
}