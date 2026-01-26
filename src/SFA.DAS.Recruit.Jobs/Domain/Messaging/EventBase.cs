namespace SFA.DAS.Recruit.Jobs.Domain.Messaging;

public abstract class EventBase : IEvent
{
    public override string ToString()
    {
        return GetType().Name;
    }
}