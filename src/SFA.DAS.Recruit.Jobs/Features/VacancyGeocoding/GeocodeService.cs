using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;
using SFA.DAS.Recruit.Jobs.OuterApi.Responses;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyGeocoding;

public interface IGeocodeService
{
    Task<Geocode?> GeocodeAsync(string postcode, CancellationToken cancellationToken = default);
}

public class GeocodeService(ILogger<GeocodeService> logger, IJobsOuterClient jobsOuterClient) : IGeocodeService
{
    public async Task<Geocode?> GeocodeAsync(string postcode, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting geo code for postcode {Postcode}", postcode);
        var apiResponse = await jobsOuterClient.GetAsync<GetGeoPointResponse>(new GetGeoCodeRequest(postcode), cancellationToken);
        if (!apiResponse.Success)
        {
            logger.LogError("Geocoding postcode {Postcode} failed with error {ErrorContent}", postcode, apiResponse.ErrorContent);
            return null;
        }

        var geoPoint = apiResponse.Payload?.GeoPoint;
        if (geoPoint is { Latitude: not null, Longitude: not null })
        {
            return new Geocode
            {
                Latitude = geoPoint.Latitude,
                Longitude = geoPoint.Longitude
            };
        }

        return null;
    }
}