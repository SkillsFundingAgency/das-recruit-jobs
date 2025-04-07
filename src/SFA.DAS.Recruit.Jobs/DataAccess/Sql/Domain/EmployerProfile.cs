using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class EmployerProfile
{
    [Key]
    public required long AccountLegalEntityId { get; init; }
    public required long AccountId { get; init; }
    public string? AboutOrganisation { get; init; }
    public string? TradingName { get; init; }
}