using System.Collections.Concurrent;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using SFA.DAS.Recruit.Jobs.Features.UserMigration;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

public class UserLocator(UserMigrationSqlRepository repository)
{
    private readonly ConcurrentDictionary<string, Guid> _users = [];

    public async Task<Guid?> LocateAsync(string id)
    {
        return await FindUserByIdAsync(id);
    }
    
    public async Task<Guid?> LocateMongoUserAsync(VacancyUser? mongoUser)
    {
        if (mongoUser?.UserId is not null)
        {
            return await FindUserByIdAndEmailAsync(mongoUser.UserId, mongoUser.Email);
        }
        
        if (mongoUser?.Email is not null)
        {
            return await FindUserByEmailAsync(mongoUser.Email);
        }

        return null;
    }

    private async Task<Guid?> FindUserByEmailAsync(string email)
    {
        if (_users.TryGetValue(email, out var userId))
        {
            return userId;
        }
            
        var users = await repository.FindUsersByEmailAsync(email);
        if (users is not { Count: 1 })
        {
            return null;
        }
        
        _users.TryAdd(email, users[0].Id);
        return users[0].Id;
    }

    private async Task<Guid?> FindUserByIdAndEmailAsync(string id, string? email)
    {
        if (_users.TryGetValue(id, out var userId))
        {
            return userId;
        }
        
        var sqlUser = await repository.FindUserByIdAndEmailAsync(id, email);
        if (sqlUser == null)
        {
            return null;
        }
        _users.TryAdd(id, sqlUser.Id);
        return sqlUser.Id;
    }
    
    private async Task<Guid?> FindUserByIdAsync(string id)
    {
        if (_users.TryGetValue(id, out var userId))
        {
            return userId;
        }
        
        var sqlUser = await repository.FindUserByIdAsync(id);
        return sqlUser?.Id;
    }
}