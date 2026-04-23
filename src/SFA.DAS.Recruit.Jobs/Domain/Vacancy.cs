namespace SFA.DAS.Recruit.Jobs.Domain;

public class Vacancy
{
    public Guid Id { get; set; }
    public required VacancyStatus Status { get; set; }
    public ClosureReason? ClosureReason { get; set; }
    public EmployerNameOption? EmployerNameOption { get; set; }
    public AvailableWhere? EmployerLocationOption { get; set; }
    public List<Address>? EmployerLocations { get; set; }
}