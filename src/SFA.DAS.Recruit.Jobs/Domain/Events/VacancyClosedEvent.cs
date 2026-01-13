using SFA.DAS.Recruit.Jobs.Domain.Events.Interfaces;
using SFA.DAS.Recruit.Jobs.Domain.Messaging;
using IEvent = NServiceBus.IEvent;

namespace SFA.DAS.Recruit.Jobs.Domain.Events;

public class VacancyClosedEvent : EventBase, IVacancyEvent, IEvent
{
    public required Guid VacancyId { get; set; } 
    public required long VacancyReference { get; set; }
}