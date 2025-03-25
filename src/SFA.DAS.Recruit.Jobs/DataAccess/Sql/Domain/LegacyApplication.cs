namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class LegacyApplication
{
    public DateTime? ApplicationDate { get; set; }
    public DateTime? BirthDate { get; set; }
    public required Guid CandidateId { get; set; }
    public required Guid Id { get; set; }
    public int? EducationFromYear { get; set; }
    public int? EducationToYear { get; set; }
    public required string ApplicationReviewDisabilityStatus { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? AddressLine4 { get; set; }
    public string? EducationInstitution { get; set; }
    public string? HobbiesAndInterests { get; set; }
    public string? Improvements { get; set; }
    public string? Phone { get; set; }
    public string? Postcode { get; set; }
    public string? Qualifications { get; set; }
    public string? Skills { get; set; }
    public string? Strengths { get; set; }
    public string? Support { get; set; }
}