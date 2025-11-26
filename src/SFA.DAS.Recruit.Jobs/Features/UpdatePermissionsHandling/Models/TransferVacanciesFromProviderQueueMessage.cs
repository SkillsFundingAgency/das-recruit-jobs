using SFA.DAS.Recruit.Jobs.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;

public class TransferVacanciesFromProviderQueueMessage
{
    public required long AccountLegalEntityId { get; set; }
    public required TransferReason TransferReason { get; set; }
    public required long Ukprn { get; set; }
}