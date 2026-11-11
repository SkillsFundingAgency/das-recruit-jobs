using SFA.DAS.Recruit.Jobs.Domain;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Close;

public sealed record PostOneVacancyCloseRequest(Guid VacancyId, ClosureReason ClosureReason);