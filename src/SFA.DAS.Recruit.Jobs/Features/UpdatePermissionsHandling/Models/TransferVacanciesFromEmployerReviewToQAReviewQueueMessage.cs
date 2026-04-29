namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;

public class TransferVacanciesFromEmployerReviewToQaReviewQueueMessage
{
    public long Ukprn { get; set; }
    public string AccountLegalEntityPublicHashedId { get; set; }
    public Guid UserRef { get; set; }
    public string UserEmailAddress { get; set; }
    public string UserName { get; set; }
}