using SFA.DAS.Recruit.Jobs.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;

public class TransferVacancyToLegalEntityQueueMessage
{
    public required Guid VacancyId { get; set; }
    public required Guid UserRef { get; set; }
    public required string UserEmailAddress { get; set; }
    public required string UserName { get; set; }
    public required TransferReason TransferReason { get; set; }
}