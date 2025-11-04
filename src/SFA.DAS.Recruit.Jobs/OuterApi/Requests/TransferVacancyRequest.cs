using SFA.DAS.Recruit.Jobs.Domain;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public record TransferVacancyRequest(Guid UserRef, string UserEmailAddress, string UserName, TransferReason TransferReason);