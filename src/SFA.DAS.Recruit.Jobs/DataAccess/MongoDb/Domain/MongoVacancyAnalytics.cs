namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
public record MongoVacancyAnalytics
{
    public DateTime AnalyticsDate { get; set; }
    public int ViewsCount { get; set; } = 0;
    public int SearchResultsCount { get; set; } = 0;
    public int ApplicationStartedCount { get; set; } = 0;
    public int ApplicationSubmittedCount { get; set; } = 0;
}