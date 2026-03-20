using SFA.DAS.Recruit.Jobs.Core.Models;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Analytics;

public record GetOneVacancyAnalyticsResponse
{
    public required long VacancyReference { get; set; }
    public DateTime UpdatedDate { get; init; }
    public List<VacancyAnalytics> Analytics { get; init; } = [];
}