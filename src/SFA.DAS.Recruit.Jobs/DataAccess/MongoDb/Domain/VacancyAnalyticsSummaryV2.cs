namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
public class VacancyAnalyticsSummaryV2 : IVacancySummaryReferenceDataItem
{
    public required string Id { get; set; }
    public string ViewType { get; set; } = "VacancyAnalyticsSummaryV2";
    public DateTime LastUpdated { get; set; }
    public long VacancyReference { get; set; }
    public List<MongoVacancyAnalytics> VacancyAnalytics { get; set; } = [];
    public DateTime? MigrationDate { get; set; }
    public bool? MigrationFailed { get; set; }
}