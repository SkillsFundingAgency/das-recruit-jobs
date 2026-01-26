using Newtonsoft.Json;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Common;

public record VacanciesToClose
{
    [JsonProperty("data")] public IEnumerable<VacancyToClose> Data { get; set; } = [];

    public record VacancyToClose
    {
        public Guid Id { get; set; }
        public DateTime ClosingDate { get; set; }
        public long VacancyReference { get; set; }
        public required string Status { get; set; }
    }
}