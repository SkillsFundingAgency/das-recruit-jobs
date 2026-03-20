using Newtonsoft.Json;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Metrics;

public record VacancyMetricResponse
{
    [JsonProperty("vacancyMetrics")] 
    public List<VacancyMetric> VacancyMetrics { get; set; } = [];

    public record VacancyMetric
    {
        public string? VacancyReference { get; set; }
        public long ViewsCount { get; init; }
        public long SearchResultsCount { get; init; }
        public long ApplicationStartedCount { get; init; }
        public long ApplicationSubmittedCount { get; init; }

        [JsonIgnore]
        public long VacancyRef => long.Parse(VacancyReference ?? "0");
    }
}