namespace SFA.DAS.Recruit.Jobs.Domain.Messaging;

public abstract record EventBase : IEvent
{
    public override string ToString()
    {
        return GetType().Name;
    }
}