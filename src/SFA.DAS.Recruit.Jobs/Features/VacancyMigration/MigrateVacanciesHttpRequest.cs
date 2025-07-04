using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

[ExcludeFromCodeCoverage]
public class MigrateVacanciesHttpRequest
{
    [JsonPropertyName("vacancyIds")]
    public required List<Guid> VacancyIds { get; init; }
}