using SFA.DAS.Recruit.Jobs.Domain.Events.Interfaces;
using SFA.DAS.Recruit.Jobs.Domain.Messaging;

namespace SFA.DAS.Recruit.Jobs.Domain.Events;

public record VacancyClosedEvent : EventBase, IVacancyEvent
{
    public required Guid VacancyId { get; init; } 
    public required long VacancyReference { get; set; }
}