using System.Net;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Features.VacancyGeocoding;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;
using SFA.DAS.Recruit.Jobs.OuterApi.Responses;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.VacancyGeocoding;

public class WhenGeocodingAPostcode
{
    [Test, MoqAutoData]
    public async Task Then_The_Geopoint_Should_Be_Returned(
        string postcode,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] GeocodeService sut)
    {
        // arrange
        var expectedGeoPoint = new GeoPoint { Latitude = 51.507351, Longitude = -0.127758 };
        GetGeoCodeRequest? capturedRequest = null;
        jobsOuterClient
            .Setup(x => x.GetAsync<GetGeoPointResponse>(It.IsAny<GetGeoCodeRequest>(), CancellationToken.None))
            .Callback<IGetRequest, CancellationToken>((x, _) => capturedRequest = x as GetGeoCodeRequest)
            .ReturnsAsync(new ApiResponse<GetGeoPointResponse>(HttpStatusCode.OK, new GetGeoPointResponse
            {
                GeoPoint = expectedGeoPoint
            }));

        // act
        var result = await sut.GeocodeAsync(postcode, CancellationToken.None);

        // assert
        capturedRequest.Should().NotBeNull();
        capturedRequest.Url.Should().Be($"geocoding/postcode/{postcode}/geopoint");
        result.Should().BeEquivalentTo(expectedGeoPoint);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Null_Points_Are_Handled(
        string postcode,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] GeocodeService sut)
    {
        // arrange
        var geoPoint = new GeoPoint { Latitude = null, Longitude = -0.127758 };
        GetGeoCodeRequest? capturedRequest = null;
        jobsOuterClient
            .Setup(x => x.GetAsync<GetGeoPointResponse>(It.IsAny<GetGeoCodeRequest>(), CancellationToken.None))
            .Callback<IGetRequest, CancellationToken>((x, _) => capturedRequest = x as GetGeoCodeRequest)
            .ReturnsAsync(new ApiResponse<GetGeoPointResponse>(HttpStatusCode.OK, new GetGeoPointResponse
            {
                GeoPoint = geoPoint
            }));

        // act
        var result = await sut.GeocodeAsync(postcode, CancellationToken.None);

        // assert
        capturedRequest.Should().NotBeNull();
        capturedRequest.Url.Should().Be($"geocoding/postcode/{postcode}/geopoint");
        result.Should().BeNull();
    }
    
    [Test, MoqAutoData]
    public async Task Then_Not_Found_Returns_Null(
        string postcode,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] GeocodeService sut)
    {
        // arrange
        jobsOuterClient
            .Setup(x => x.GetAsync<GetGeoPointResponse>(It.IsAny<GetGeoCodeRequest>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<GetGeoPointResponse>(HttpStatusCode.NotFound, null));

        // act
        var result = await sut.GeocodeAsync(postcode, CancellationToken.None);

        // assert
        result.Should().BeNull();
    }
}