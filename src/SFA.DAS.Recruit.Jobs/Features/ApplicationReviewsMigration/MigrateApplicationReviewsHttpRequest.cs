using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

public class MigrateApplicationReviewsHttpRequest
{
    [JsonPropertyName("applicationReviewIds")]
    public required List<Guid> ApplicationReviewIds { get; init; }
}