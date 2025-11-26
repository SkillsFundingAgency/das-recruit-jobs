using SFA.DAS.Recruit.Jobs.Domain;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public record TransferVacancyRequest(TransferReason TransferReason);