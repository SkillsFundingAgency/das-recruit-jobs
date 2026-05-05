// ReSharper disable once CheckNamespace - This is required for backwards compaitibility
namespace Esfa.Recruit.Vacancies.Client.Domain.Events;

public class VacancyClosedEvent
{
    public Guid VacancyId { get; set; }
    public long VacancyReference { get; set; }
}