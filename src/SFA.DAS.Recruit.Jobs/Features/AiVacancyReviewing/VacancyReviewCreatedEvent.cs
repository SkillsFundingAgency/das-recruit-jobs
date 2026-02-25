// ReSharper disable once CheckNamespace -- THIS MUST STAY LIKE THIS TO MATCH THE EVENT FROM RECRUIT INNER
namespace SFA.DAS.Recruit.Api.Core.Events;

public class VacancyReviewCreatedEvent
{
    public Guid VacancyId { get; set; }
    public Guid VacancyReviewId { get; set; }
}