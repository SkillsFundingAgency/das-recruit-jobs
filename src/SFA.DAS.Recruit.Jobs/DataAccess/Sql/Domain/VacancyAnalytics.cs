using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
public class VacancyAnalytics
{
    public required long VacancyReference { get; set; }
    public DateTime UpdatedDate { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    [MaxLength(4000)]
    public string Analytics { get; set; } = string.Empty;

    private bool TryGetAnalyticsData(out Core.Models.VacancyAnalytics? data)
    {
        try
        {
            data = JsonSerializer.Deserialize<Core.Models.VacancyAnalytics>(Analytics);
            return data is not null;
        }
        catch
        {
            data = null;
            return false;
        }
    }

    [NotMapped]
    public Core.Models.VacancyAnalytics? AnalyticsData
    {
        get
        {
            _ = TryGetAnalyticsData(out var data);
            return data;
        }
    }
}