using Newtonsoft.Json;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Common;

public record VacanciesToArchive
{
    [JsonProperty("data")]
    public IEnumerable<VacancyToArchive> Data { get; set; } = [];

    public sealed record VacancyToArchive
    {
        public required Guid Id { get; set; }
        public DateTime ClosingDate { get; set; }
        public long VacancyReference { get; set; }
        public required VacancyStatus Status { get; set; }
    }
}