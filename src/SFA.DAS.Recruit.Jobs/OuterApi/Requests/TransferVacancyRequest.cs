using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling.Domain;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public record TransferVacancyRequest(long VacancyReference, Guid UserRef, string UserEmailAddress, string UserName, TransferReason TransferReason);