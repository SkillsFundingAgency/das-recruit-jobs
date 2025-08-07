using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

[ExcludeFromCodeCoverage]
public class MigrateApplicationReviewsHttpRequest
{
    [JsonPropertyName("vacancyReference")]
    public required long VacancyReference { get; init; }
}