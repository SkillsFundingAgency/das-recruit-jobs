namespace SFA.DAS.Recruit.Jobs.Domain;

public class Vacancy
{
    public Guid Id { get; set; }
    public required VacancyStatus Status { get; set; }
    public ClosureReason? ClosureReason { get; set; }
}