using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using SFA.DAS.Recruit.Jobs.Features.UserMigration;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

public class UserLocator(UserMigrationSqlRepository repository)
{
    private readonly Dictionary<string, Guid> _users = [];
    
    public async Task<Guid?> Locate(VacancyUser? mongoUser)
    {
        if (mongoUser?.UserId is not null)
        {
            return await FindUserById(mongoUser.UserId, mongoUser.Email);
        }
        
        if (mongoUser?.Email is not null)
        {
            return await FindUserByEmail(mongoUser.Email);
        }

        return null;
    }

    private async Task<Guid?> FindUserByEmail(string email)
    {
        if (_users.TryGetValue(email, out var userId))
        {
            return userId;
        }
            
        var users = await repository.FindUsersByEmail(email);
        if (users is not { Count: 1 })
        {
            return null;
        }
            
        _users.Add(email, users[0].Id);
        return users[0].Id;
    }

    private async Task<Guid?> FindUserById(string id, string? email)
    {
        if (_users.TryGetValue(id, out var userId))
        {
            return userId;
        }
        
        var sqlUser = await repository.FindUser(id, email);
        if (sqlUser == null)
        {
            return null;
        }
        _users.Add(id, sqlUser.Id);
        return sqlUser.Id;
    }
}