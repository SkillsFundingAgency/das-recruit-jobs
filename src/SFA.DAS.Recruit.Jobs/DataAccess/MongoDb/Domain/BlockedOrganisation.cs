namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

public class BlockedOrganisation
{
    public Guid Id { get; set; }
    public OrganisationType OrganisationType { get; set; }
    public string OrganisationId { get; set; }
    public BlockedStatus BlockedStatus { get; set; }
    public VacancyUser? UpdatedByUser { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string? Reason { get; set; }
    
    public DateTime? MigrationDate { get; set; }
    public bool? MigrationFailed { get; set; }
}

public enum OrganisationType
{
    Employer,
    Provider
}

public enum BlockedStatus 
{
    Blocked,
    Unblocked
}