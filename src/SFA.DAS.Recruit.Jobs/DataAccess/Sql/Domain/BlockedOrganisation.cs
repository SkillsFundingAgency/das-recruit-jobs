using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class BlockedOrganisation
{
    internal static readonly BlockedOrganisation None = new()
    {
        Id = Guid.Empty,
        BlockedStatus = BlockedStatus.Blocked,
        OrganisationType = OrganisationType.Employer,
        OrganisationId = string.Empty,
        Reason = string.Empty,
        UpdatedByUserId = Guid.Empty,
        UpdatedDate = DateTime.MinValue,
    };
    
    public required Guid Id { get; init; }
    public required BlockedStatus BlockedStatus { get; init; }
    public required string OrganisationId { get; init; }
    public required OrganisationType OrganisationType { get; init; }
    public required string Reason { get; init; }
    public required Guid UpdatedByUserId { get; init; }
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