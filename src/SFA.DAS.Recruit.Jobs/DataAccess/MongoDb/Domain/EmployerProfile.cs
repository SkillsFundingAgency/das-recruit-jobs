namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

public class EmployerProfile
{
    public string Id { get; set; }
    public string EmployerAccountId { get; set; }
    public string AboutOrganisation { get; set; }
    public string OrganistationWebsiteUrl { get; set; }
    public string TradingName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public VacancyUser LastUpdatedBy { get; set; }
    public string AccountLegalEntityPublicHashedId { get; set; }
    public IList<Address> OtherLocations { get; set; } = new List<Address>();
    public DateTime? MigrationDate { get; set; }
    public bool? MigrationFailed { get; set; }
}