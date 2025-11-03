namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Domain;

public class TransferVacancyToLegalEntityQueueMessage
{
    public required long VacancyReference { get; set; }
    public required Guid UserRef { get; set; }
    public required string UserEmailAddress { get; set; }
    public required string UserName { get; set; }
    public required TransferReason TransferReason { get; set; }
}