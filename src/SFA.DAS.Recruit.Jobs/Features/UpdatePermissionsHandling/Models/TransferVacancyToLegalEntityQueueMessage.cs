using SFA.DAS.Recruit.Jobs.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;

public class TransferVacancyToLegalEntityQueueMessage
{
    public required Guid VacancyId { get; set; }
    public required TransferReason TransferReason { get; set; }
}