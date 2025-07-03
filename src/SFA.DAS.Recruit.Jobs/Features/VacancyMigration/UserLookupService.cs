using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

public interface IUser
{
    Guid Id { get; }
}

public record User(Guid Id) : IUser
{
    public static readonly IUser None = new User(Guid.Empty);
}

public class UserLookupService
{
    private readonly Dictionary<string, IUser> _usersCacheByEmail = new ();
    private readonly Dictionary<string, IUser> _usersCacheByUserId = new ();
    
    public async Task<IUser> LookupUserAsync(VacancyUser? vacancyUser)
    {
        if (vacancyUser == null)
        {
            return User.None;
        }

        if (TryQueryCache(vacancyUser, out var user))
        {
            return user;
        }

        return await GetUserAndPopulateCache(vacancyUser);
    }

    private async Task<IUser> GetUserAndPopulateCache(VacancyUser vacancyUser)
    {
        // TODO: lookup into the SQL db for the user here
        return User.None;
    }

    private bool TryQueryCache(VacancyUser user, out IUser result)
    {
        if (!string.IsNullOrWhiteSpace(user.UserId) && _usersCacheByUserId.TryGetValue(user.Email, out result!))
        {
            return true;
        }
        
        if (!string.IsNullOrWhiteSpace(user.Email) && _usersCacheByEmail.TryGetValue(user.Email, out result!))
        {
            return true;
        }
         
        result = User.None;
        return false;
    }
}