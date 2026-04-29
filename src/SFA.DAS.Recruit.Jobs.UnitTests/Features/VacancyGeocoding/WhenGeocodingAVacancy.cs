using System.Net;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.Features.VacancyGeocoding;
using SFA.DAS.Recruit.Jobs.Features.VacancyGeocoding.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.VacancyGeocoding;

public class WhenGeocodingAVacancy
{
    [Test, MoqAutoData]
    public async Task Then_Vacancies_Advertising_Across_England_Are_Not_Geocoded(
        Vacancy vacancy,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IGeocodeService> geocodeService,
        [Greedy] GeocodeVacancyHandler sut)
    {
        // arrange
        vacancy.EmployerLocationOption = AvailableWhere.AcrossEngland;
        vacancy.EmployerLocations = [];
        jobsOuterClient
            .Setup(x => x.GetAsync<DataResponse<Vacancy?>>(It.IsAny<GetVacancyByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<DataResponse<Vacancy?>>(HttpStatusCode.OK, new DataResponse<Vacancy?>(vacancy)));

        // act
        await sut.HandleAsync(vacancy.Id, CancellationToken.None);

        // assert
        geocodeService.Verify(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        jobsOuterClient.Verify(x => x.PostAsync(It.IsAny<PostGeocodedAddresses>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Vacancy_With_Multiple_Locations_Is_Geocoded(
        Vacancy vacancy,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IGeocodeService> geocodeService,
        [Greedy] GeocodeVacancyHandler sut)
    {
        // arrange
        vacancy.EmployerLocationOption = AvailableWhere.MultipleLocations;
        vacancy.EmployerLocations?.ForEach(location =>
        {
            location.Latitude = null;
            location.Longitude = null;
        });
        GetVacancyByIdRequest? capturedRequest = null;
        jobsOuterClient
            .Setup(x => x.GetAsync<DataResponse<Vacancy?>>(It.IsAny<GetVacancyByIdRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IGetRequest, CancellationToken>((x, _) => capturedRequest = x as GetVacancyByIdRequest)
            .ReturnsAsync(new ApiResponse<DataResponse<Vacancy?>>(HttpStatusCode.OK, new DataResponse<Vacancy?>(vacancy)));
        
        PostGeocodedAddresses? capturedPostRequest = null;
        jobsOuterClient
            .Setup(x => x.PostAsync(It.IsAny<PostGeocodedAddresses>(), It.IsAny<CancellationToken>()))
            .Callback<IPostRequest, CancellationToken>((x, _) => capturedPostRequest = x as PostGeocodedAddresses)
            .ReturnsAsync(new ApiResponse(HttpStatusCode.OK));
        
        var expectedGeocode = new Geocode { Latitude = 51.507351, Longitude = -0.127758 };
        geocodeService
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGeocode);

        // act
        await sut.HandleAsync(vacancy.Id, CancellationToken.None);

        // assert
        capturedRequest.Should().NotBeNull();
        capturedRequest.Url.Should().Be($"vacancies/{vacancy.Id}");
        
        capturedPostRequest.Should().NotBeNull();
        capturedPostRequest.Url.Should().Be($"geocoding/vacancies/{vacancy.Id}/geocoded");
        capturedPostRequest.Data.Should().BeEquivalentTo(vacancy.EmployerLocations);
        
        geocodeService.Verify(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(vacancy.EmployerLocations!.Count));
        jobsOuterClient.Verify(x => x.PostAsync(It.IsAny<PostGeocodedAddresses>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Vacancy_With_One_Location_Is_Geocoded(
        Vacancy vacancy,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IGeocodeService> geocodeService,
        [Greedy] GeocodeVacancyHandler sut)
    {
        // arrange
        vacancy.EmployerLocationOption = AvailableWhere.OneLocation;
        vacancy.EmployerLocations = [vacancy.EmployerLocations!.First()];
        vacancy.EmployerLocations[0].Latitude = null;
        vacancy.EmployerLocations[0].Longitude = null;

        GetVacancyByIdRequest? capturedRequest = null;
        jobsOuterClient
            .Setup(x => x.GetAsync<DataResponse<Vacancy?>>(It.IsAny<GetVacancyByIdRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IGetRequest, CancellationToken>((x, _) => capturedRequest = x as GetVacancyByIdRequest)
            .ReturnsAsync(new ApiResponse<DataResponse<Vacancy?>>(HttpStatusCode.OK, new DataResponse<Vacancy?>(vacancy)));
        
        PostGeocodedAddresses? capturedPostRequest = null;
        jobsOuterClient
            .Setup(x => x.PostAsync(It.IsAny<PostGeocodedAddresses>(), It.IsAny<CancellationToken>()))
            .Callback<IPostRequest, CancellationToken>((x, _) => capturedPostRequest = x as PostGeocodedAddresses)
            .ReturnsAsync(new ApiResponse(HttpStatusCode.OK));
        
        var expectedGeocode = new Geocode { Latitude = 51.507351, Longitude = -0.127758 };
        geocodeService
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGeocode);

        // act
        await sut.HandleAsync(vacancy.Id, CancellationToken.None);

        // assert
        capturedRequest.Should().NotBeNull();
        capturedRequest.Url.Should().Be($"vacancies/{vacancy.Id}");
        
        capturedPostRequest.Should().NotBeNull();
        capturedPostRequest.Url.Should().Be($"geocoding/vacancies/{vacancy.Id}/geocoded");
        capturedPostRequest.Data.Should().BeEquivalentTo(vacancy.EmployerLocations);
        
        geocodeService.Verify(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        jobsOuterClient.Verify(x => x.PostAsync(It.IsAny<PostGeocodedAddresses>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Only_Address_That_Have_Postcodes_Are_Geocoded(
        Vacancy vacancy,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IGeocodeService> geocodeService,
        [Greedy] GeocodeVacancyHandler sut)
    {
        // arrange
        vacancy.EmployerLocationOption = AvailableWhere.MultipleLocations;
        vacancy.EmployerLocations![0].Postcode = "";
        jobsOuterClient
            .Setup(x => x.GetAsync<DataResponse<Vacancy?>>(It.IsAny<GetVacancyByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<DataResponse<Vacancy?>>(HttpStatusCode.OK, new DataResponse<Vacancy?>(vacancy)));
        
        PostGeocodedAddresses? capturedPostRequest = null;
        jobsOuterClient
            .Setup(x => x.PostAsync(It.IsAny<PostGeocodedAddresses>(), It.IsAny<CancellationToken>()))
            .Callback<IPostRequest, CancellationToken>((x, _) => capturedPostRequest = x as PostGeocodedAddresses)
            .ReturnsAsync(new ApiResponse(HttpStatusCode.OK));
        
        geocodeService
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Geocode { Latitude = 51.507351, Longitude = -0.127758 });

        // act
        await sut.HandleAsync(vacancy.Id, CancellationToken.None);

        // assert
        geocodeService.Verify(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(vacancy.EmployerLocations!.Count - 1));
        capturedPostRequest.Should().NotBeNull();
        capturedPostRequest.Data.Should().BeEquivalentTo(vacancy.EmployerLocations);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Anonymous_Vacancies_Only_Have_Their_Outcode_Geocoded(
        Vacancy vacancy,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IGeocodeService> geocodeService,
        [Greedy] GeocodeVacancyHandler sut)
    {
        // arrange
        vacancy.EmployerNameOption = EmployerNameOption.Anonymous;
        vacancy.EmployerLocationOption = AvailableWhere.OneLocation;
        vacancy.EmployerLocations = [vacancy.EmployerLocations!.First()];
        vacancy.EmployerLocations[0].Latitude = null;
        vacancy.EmployerLocations[0].Longitude = null;
        vacancy.EmployerLocations[0].Postcode = "SW1 1AA";

        jobsOuterClient
            .Setup(x => x.GetAsync<DataResponse<Vacancy?>>(It.IsAny<GetVacancyByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<DataResponse<Vacancy?>>(HttpStatusCode.OK, new DataResponse<Vacancy?>(vacancy)));
        
        jobsOuterClient
            .Setup(x => x.PostAsync(It.IsAny<PostGeocodedAddresses>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse(HttpStatusCode.OK));
        
        string? capturedPostcode = null;
        geocodeService
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((postcode, _) => capturedPostcode = postcode)
            .ReturnsAsync(new Geocode { Latitude = 51.507351, Longitude = -0.127758 });

        // act
        await sut.HandleAsync(vacancy.Id, CancellationToken.None);

        // assert
        capturedPostcode.Should().Be("SW1");
    }
}