using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

[ExcludeFromCodeCoverage]
public class VacancyUser
{
    public string UserId { get; set; }
    public string DfEUserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public long? Ukprn { get; set; }
}