namespace SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing;

public class VacancyReviewCreatedEvent
{
    public Guid VacancyId { get; set; }
    public Guid VacancyReviewId { get; set; }
}