using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AiReviewStatus
{
    Pending,
    Passed,
    Failed,
}