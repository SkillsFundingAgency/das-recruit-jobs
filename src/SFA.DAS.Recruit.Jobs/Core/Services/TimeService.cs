namespace SFA.DAS.Recruit.Jobs.Core.Services;

public interface ITimeService
{
    TimeZoneInfo GmtTimeZoneInfo { get; }
    DateTime UtcNow { get; }
    
    DateTime GmtNow { get; }
    DateTime UtcToGmt(DateTime value);
}

internal class TimeService : ITimeService
{
    public TimeZoneInfo GmtTimeZoneInfo { get; } = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime GmtNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GmtTimeZoneInfo);

    public DateTime UtcToGmt(DateTime value)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(value, GmtTimeZoneInfo);
    }
}