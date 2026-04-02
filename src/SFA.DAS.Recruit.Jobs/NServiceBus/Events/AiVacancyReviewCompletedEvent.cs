using SFA.DAS.Recruit.Jobs.OuterApi.Common;

// ReSharper disable once CheckNamespace -- THIS MUST STAY LIKE THIS TO MATCH THE EVENT FROM VACANCY AI INNER
namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Events;

public sealed record AiVacancyReviewCompletedEvent(Guid VacancyId, Guid VacancyReviewId, AiReviewStatus ReviewStatus, bool ManualReviewRequired);