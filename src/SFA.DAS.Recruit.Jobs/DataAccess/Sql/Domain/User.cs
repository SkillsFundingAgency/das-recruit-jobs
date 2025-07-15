using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class User
{
    public static readonly User None = new ();
    
    public Guid Id { get; set; }
    public string IdamsUserId { get; set; } 
    public UserType UserType { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastSignedInDate { get; set; }
    public List<string> EmployerAccountIds { get; set; } = [];
    public long? Ukprn { get; set; }
    public DateTime? TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn { get; set; }
    public DateTime? ClosedVacanciesBlockedProviderAlertDismissedOn { get; set; }
    public DateTime? TransferredVacanciesBlockedProviderAlertDismissedOn { get; set; }
    public DateTime? ClosedVacanciesWithdrawnByQaAlertDismissedOn { get; set; }
    public string DfEUserId { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserType
{
    Employer,
    Provider
}