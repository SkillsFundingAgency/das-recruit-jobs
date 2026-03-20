using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class BlockedOrganisation
{
    public required Guid Id { get; init; }
    public required BlockedStatus BlockedStatus { get; init; }
    public required string OrganisationId { get; init; }
    public required OrganisationType OrganisationType { get; init; }
    public required string Reason { get; init; }
    public required string UpdatedByUserId { get; init; }
    public required string UpdatedByUserEmail { get; init; }
    public required DateTime UpdatedDate { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrganisationType
{
    Employer,
    Provider
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BlockedStatus 
{
    Blocked,
    Unblocked
}