using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApplicationMethod
{
    ThroughFindAnApprenticeship,
    ThroughExternalApplicationSite,
    ThroughFindATraineeship
}