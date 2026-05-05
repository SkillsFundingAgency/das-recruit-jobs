using System.Net;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;
using SFA.DAS.Recruit.Jobs.Services;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Services;

public class WhenGettingAVacancyById
{
    [Test, MoqAutoData]
    public async Task Then_The_Call_Succeeds_And_The_Items_Are_Returned(
        Guid id,
        DataResponse<Vacancy?> dataResponse,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] VacancyService sut)
    {
        // arrange
        var expectedRequest = new GetVacancyByIdRequest(id);
        GetVacancyByIdRequest? capturedRequest = null;
        jobsOuterClient
            .Setup(x => x.GetAsync<DataResponse<Vacancy?>>(It.IsAny<GetVacancyByIdRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IGetRequest, CancellationToken>((x, _) => capturedRequest = x as GetVacancyByIdRequest)
            .ReturnsAsync(new ApiResponse<DataResponse<Vacancy?>>(HttpStatusCode.OK, dataResponse));

        // act
        var response = await sut.GetByIdAsync(id, CancellationToken.None);

        // assert
        response!.Id.Should().Be(dataResponse.Data!.Id);
        response.ClosureReason.Should().Be(dataResponse.Data.ClosureReason);
        response.Status.Should().Be(dataResponse.Data.Status);
        
        capturedRequest.Should().BeEquivalentTo(expectedRequest);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Call_Returns_Null_When_The_Vacancy_Is_Not_Found(
        Guid id,
        DataResponse<Vacancy?> dataResponse,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] VacancyService sut)
    {
        // arrange
        jobsOuterClient
            .Setup(x => x.GetAsync<DataResponse<Vacancy?>>(It.IsAny<GetVacancyByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<DataResponse<Vacancy?>>(HttpStatusCode.NotFound, null));

        // act
        var response = await sut.GetByIdAsync(id, CancellationToken.None);

        // assert
        response.Should().BeNull();
    }
    
    [Test, MoqAutoData]
    public async Task Then_An_Exception_Is_Thrown_When_The_Call_Fails(
        Guid id,
        DataResponse<Vacancy?> dataResponse,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] VacancyService sut)
    {
        // arrange
        jobsOuterClient
            .Setup(x => x.GetAsync<DataResponse<Vacancy?>>(It.IsAny<GetVacancyByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<DataResponse<Vacancy?>>(HttpStatusCode.BadRequest, dataResponse));

        // act
        var action = async () => await sut.GetByIdAsync(id, CancellationToken.None);

        // assert
        await action.Should().ThrowAsync<ApiException>();
    }
}