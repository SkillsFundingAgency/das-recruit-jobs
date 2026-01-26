using Newtonsoft.Json;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Common;

public record StaleVacancies
{
    [JsonProperty("data")] public IEnumerable<StaleVacancyToClose> Data { get; set; } = [];

    public record StaleVacancyToClose
    {
        public required Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? VacancyReference { get; set; }
        public required VacancyStatus Status { get; set; }
    }
}