using SFA.DAS.Recruit.Jobs.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;

public class TransferVacanciesFromProviderQueueMessage
{
    public required long AccountLegalEntityId { get; set; }
    public required long EmployerAccountId { get; set; }
    public required TransferReason TransferReason { get; set; }
    public required long Ukprn { get; set; }
    public required string UserEmailAddress { get; set; }
    public required string UserName { get; set; }
    public required Guid UserRef { get; set; }
}