﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

[ExcludeFromCodeCoverage]
public class MigrateApplicationReviewsHttpRequest
{
    [JsonPropertyName("applicationReviewIds")]
    public required List<Guid> ApplicationReviewIds { get; init; }
}