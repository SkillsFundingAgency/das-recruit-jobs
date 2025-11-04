namespace SFA.DAS.Recruit.Jobs.Core.Infrastructure;

public class QueueItem<T>
{
    public required T Payload { get; set; }
}