namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
public interface IVacancySummaryReferenceDataItem
{
    public string Id { get; set; }
    public string ViewType { get; set; }
    public DateTime LastUpdated { get; set; }
}
