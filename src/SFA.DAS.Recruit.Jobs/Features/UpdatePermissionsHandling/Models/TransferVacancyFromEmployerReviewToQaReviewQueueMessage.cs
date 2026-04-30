namespace SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Models;

public sealed record TransferVacancyFromEmployerReviewToQaReviewQueueMessage(Guid VacancyId, Guid UserReference, string UserName, string UserEmailAddress);