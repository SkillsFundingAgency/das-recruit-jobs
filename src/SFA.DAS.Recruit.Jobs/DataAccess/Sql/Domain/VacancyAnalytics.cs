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

    private bool _parsed = false;
    private List<VacancyAnalyticsItem>? _cache;

    private void ParseIfNeeded()
    {
        if (_parsed)
            return;

        _parsed = true;

        try
        {
            _cache = JsonSerializer.Deserialize<List<VacancyAnalyticsItem>>(Analytics)
                     ?? [];
        }
        catch
        {
            _cache = [];
        }
    }

    [NotMapped]
    public List<VacancyAnalyticsItem> AnalyticsData
    {
        get
        {
            ParseIfNeeded();
            return _cache!;
        }
    }
}

public class VacancyAnalyticsItem
{
    public DateTime AnalyticsDate { get; set; }
    public int ViewsCount { get; set; }
    public int SearchResultsCount { get; set; }
    public int ApplicationStartedCount { get; set; }
    public int ApplicationSubmittedCount { get; set; }
}