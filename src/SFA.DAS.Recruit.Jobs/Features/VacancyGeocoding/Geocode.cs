namespace SFA.DAS.Recruit.Jobs.Features.VacancyGeocoding;

public class Geocode
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public override string ToString()
    {
        return $"{{\"Latitude\":{Latitude}, \"Longitude\":{Longitude}}}";
    }
}