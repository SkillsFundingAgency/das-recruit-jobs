namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

public class User
{
    public Guid Id { get; set; }
    public string IdamsUserId { get; set; } 
    public UserType UserType { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastSignedInDate { get; set; }
    public IList<string>? EmployerAccountIds { get; set; } = new List<string>();
    public long? Ukprn { get; set; }
    public DateTime? TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn { get; set; }
    public DateTime? ClosedVacanciesBlockedProviderAlertDismissedOn { get; set; }
    public DateTime? TransferredVacanciesBlockedProviderAlertDismissedOn { get; set; }
    public DateTime? ClosedVacanciesWithdrawnByQaAlertDismissedOn { get; set; }
    public string DfEUserId { get; set; }
    
    public DateTime? MigrationDate { get; set; }
    public bool? MigrationFailed { get; set; }
}

public enum UserType
{
    Employer,
    Provider
}