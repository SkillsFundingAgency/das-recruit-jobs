namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
public class VacancyAnalyticsSummaryV2 : IVacancySummaryReferenceDataItem
{
    public string Id { get; set; }
    public string ViewType { get; set; } = "VacancyAnalyticsSummaryV2";
    public DateTime LastUpdated { get; set; }
    public required long VacancyReference { get; set; }
    public List<MongoVacancyAnalytics> VacancyAnalytics { get; set; } = [];
}