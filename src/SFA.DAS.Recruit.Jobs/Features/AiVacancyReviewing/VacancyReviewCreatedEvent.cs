// ReSharper disable once CheckNamespace -- THIS MUST STAY LIKE THIS TO MATCH THE EVENT FROM RECRUIT INNER
namespace SFA.DAS.Recruit.Api.Core.Events;

public sealed record VacancyReviewCreatedEvent(Guid VacancyReviewId, Guid VacancyId) : IEvent;