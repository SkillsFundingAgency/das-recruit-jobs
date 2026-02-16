// ReSharper disable once CheckNamespace -- THIS MUST STAY LIKE THIS TO MATCH THE EVENT FROM RECRUIT
namespace Esfa.Recruit.Vacancies.Client.Domain.Events;

public record VacancyClosedEvent : ICommand
{
    public required Guid VacancyId { get; init; }
    public required long VacancyReference { get; set; }
}