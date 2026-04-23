using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyGeocoding.Handlers;

public interface IGeocodeVacancyHandler
{
    Task HandleAsync(Guid vacancyId, CancellationToken cancellationToken);
}

public class GeocodeVacancyHandler(
    ILogger<GeocodeVacancyHandler> logger,
    IJobsOuterClient jobsOuterClient,
    IGeocodeService geocodeService): IGeocodeVacancyHandler
{
    private const int PostcodeMinLength = 5;
    private const int IncodeLength = 3;
    
    public async Task HandleAsync(Guid vacancyId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Geocoding: vacancy '{VacancyId}'", vacancyId);
        
        var getVacancyResponse = await jobsOuterClient.GetAsync<DataResponse<Vacancy?>>(new GetVacancyByIdRequest(vacancyId), cancellationToken);
        getVacancyResponse.ThrowIfErrored("Geocoding: could not fetch vacancy '{vacancyId}'");

        var vacancy = getVacancyResponse.Payload!.Data!;
        
        switch (vacancy.EmployerLocationOption)
        {
            case AvailableWhere.AcrossEngland:
                logger.LogInformation("Geocoding: skipping vacancy '{vacancyId}' (across England)", vacancyId);
                return;
            case AvailableWhere.OneLocation when vacancy.EmployerLocations is { Count: > 0 }:
            case AvailableWhere.MultipleLocations when vacancy.EmployerLocations is { Count: > 0 }:
            {
                await GeocodeVacancy(vacancy);
                break;
            }
            default:
            {
                logger.LogWarning("Geocoding: vacancy '{vacancyId}' has no locations", vacancyId);
                break;
            }
        }
    }

    private async Task GeocodeVacancy(Vacancy vacancy)
    {
        var locations = vacancy.EmployerLocations!;
        var isAnonymous = vacancy.EmployerNameOption == EmployerNameOption.Anonymous;

        var locationsNeedingGeocoding = locations
            .Where(x => !string.IsNullOrWhiteSpace(x.Postcode))
            .ToList();
        
        if (locationsNeedingGeocoding is { Count: 0 })
        {
            return;
        }
        
        // setup a dictionary with the postcode mapped to the lookup task
        var lookups = locationsNeedingGeocoding
            .Select(x => isAnonymous ? PostcodeAsOutcode(x) : x.Postcode)
            .Distinct()
            .ToDictionary(x => x!, x => TryGeocode(vacancy.Id, x!));
        
        // wait for all the lookups to complete
        await Task.WhenAll(lookups.Select(x => x.Value));
        
        // did any tasks fail to return a geocode?
        var failedLookups = lookups.Where(x => x.Value.Result is null).Select(x => x.Key).ToList();
        if (failedLookups is { Count: > 0 })
        {
            logger.LogWarning("Geocode: vacancy {vacancyId} - failed to lookup geocode data for the following postcodes {postcodes}", vacancy.Id, string.Join(", ", failedLookups));
        }
            
        // process the successful lookups
        var postcodeLookups = lookups.Where(x => x.Value.Result is not null).ToDictionary(x => x.Key, x => x.Value.Result);
        if (postcodeLookups is { Count: 0 })
        {
            // all lookups failed
            return;
        }
        
        locations.ForEach(location =>
        {
            if (string.IsNullOrWhiteSpace(location.Postcode))
            {
                return;
            }

            var postcode = isAnonymous ? PostcodeAsOutcode(location) : location.Postcode;
            if (postcodeLookups.TryGetValue(postcode, out var geocode) && geocode is not null)
            {
                location.Latitude = geocode!.Latitude;
                location.Longitude = geocode.Longitude;
            }
        });
        
        var response = await jobsOuterClient.PostAsync(new PostGeocodedAddresses(vacancy.Id, locationsNeedingGeocoding));
        if (!response.Success)
        {
            logger.LogError("Geocode: failure updating geocoded addresses for vacancy '{VacancyId}': {ErrorContent}", vacancy.Id, response.ErrorContent);
        }
    }
    
    public static string PostcodeAsOutcode(Address address)
    {
        var postcode = address.Postcode!.Replace(" ", "");
        return postcode.Length < PostcodeMinLength
            ? postcode
            : postcode[..^IncodeLength]!;
    }
    
    private async Task<Geocode?> TryGeocode(Guid vacancyId, string postcode)
    {
        try
        {
            return await geocodeService.Geocode(postcode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Geocode: vacancy {vacancyId} - error thrown whilst geocoding postcode {postcode}", vacancyId, postcode);
        }

        return null;
    }
}