namespace SFA.DAS.Recruit.Jobs.OuterApi.Responses;

public class GetGeoPointResponse
{
    public required GeoPoint GeoPoint { get; set; }
}

public class GeoPoint
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}