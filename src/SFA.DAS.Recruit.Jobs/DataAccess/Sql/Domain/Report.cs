namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public record Report
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string Name { get; set; } = null!;
    public ReportType Type { get; set; }
    public ReportOwnerType OwnerType { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public int DownloadCount { get; set; } = 0;
    public string DynamicCriteria { get; set; } = null!;
}
public enum ReportOwnerType
{
    Provider,
    Qa
}
public enum ReportType
{
    ProviderApplications,
    QaApplications
}