using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public record EmployerProfileAddress
{
    [Key]
    public int Id { get; set; }
    public required long AccountLegalEntityId { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? AddressLine4 { get; set; }
    public required string Postcode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}