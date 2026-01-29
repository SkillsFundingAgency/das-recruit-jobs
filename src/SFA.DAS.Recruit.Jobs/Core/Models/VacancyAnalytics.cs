namespace SFA.DAS.Recruit.Jobs.Core.Models;

public record VacancyAnalytics
{
    public DateTime AnalyticsDate { get; init; }
    public long ViewsCount { get; init; } = 0;
    public long SearchResultsCount { get; init; } = 0;
    public long ApplicationStartedCount { get; init; } = 0;
    public long ApplicationSubmittedCount { get; init; } = 0;

    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}