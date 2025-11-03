namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Domain;

public class TransferVacanciesFromProviderQueueMessage
{
    public required string AccountLegalEntityPublicHashedId { get; set; }
    public required string EmployerAccountId { get; set; }
    public required TransferReason TransferReason { get; set; }
    public required long Ukprn { get; set; }
    public required string UserEmailAddress { get; set; }
    public required string UserName { get; set; }
    public required Guid UserRef { get; set; }
}