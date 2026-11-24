using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeoCodeMethod
{
    Unspecified,
    ExistingVacancy,
    PostcodesIo,
    Loqate,
    PostcodesIoOutcode,
    OuterApi,
    FailedToGeoCode
}