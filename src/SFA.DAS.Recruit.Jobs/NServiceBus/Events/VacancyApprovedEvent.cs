// ReSharper disable once CheckNamespace - This is required for backwards compaitibility
namespace Esfa.Recruit.Vacancies.Client.Domain.Events;

public class VacancyApprovedEvent
{
    public string AccountLegalEntityPublicHashedId { get; set; }
    public long Ukprn { get; set; }
    public Guid VacancyId { get; set; }
    public long VacancyReference { get; set; }
}