namespace SFA.DAS.Recruit.Jobs.Domain.Events.Interfaces;

public interface IVacancyEvent
{
    Guid VacancyId { get; }
}