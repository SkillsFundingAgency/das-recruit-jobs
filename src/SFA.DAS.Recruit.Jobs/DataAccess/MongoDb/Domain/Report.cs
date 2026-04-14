namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

public class ReportParameters
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public long? Ukprn { get; set; }
}

public class Report
{
    public Guid Id { get; set; }
    public string ReportName { get; set; }
    public string RerportType { get; set; }
    public VacancyUser RequestedBy { get; set; }
    public DateTime RequestedOn { get; set; }
    public int DownloadCount { get; set; }
    public ReportParameters Parameters { get; set; }
    public DateTime? MigrationDate { get; set; }
    public bool? MigrationFailed { get; set; }
}