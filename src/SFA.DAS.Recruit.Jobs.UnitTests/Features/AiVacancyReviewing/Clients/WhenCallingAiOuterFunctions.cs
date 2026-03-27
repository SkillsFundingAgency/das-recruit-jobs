using System.Net;
using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.AiVacancyReviewing.Clients;

public class WhenCallingAiOuterFunctions
{
    private readonly JsonSerializerOptions _serialiserOptions = new();
    
    private RecruitAiOuterClient CreateSut(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var config = new RecruitJobsOuterApiConfiguration
        {
            BaseUrl = "http://localhost:8080",
            Key = "1234567890"
        };

        return new RecruitAiOuterClient(httpClient, config, _serialiserOptions);
    }
    
    [Test, MoqAutoData]
    public async Task Then_ReviewVacancyAsync_Url_Is_Correct(Guid vacancyId, Guid vacancyReviewId)
    {
        // arrange
        var handler = new MockHttpMessageHandler([new HttpResponseMessage(HttpStatusCode.OK)]);
        var sut = CreateSut(handler);

        // act
        await sut.ReviewVacancyAsync(vacancyId, vacancyReviewId, CancellationToken.None);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().Be(new Uri($"http://localhost:8080/ai/vacancies/{vacancyId}/review"));
        request.Method.Should().Be(HttpMethod.Post);
        
        var content = await request.Content!.ReadAsStringAsync();
        content.Should().Be(JsonSerializer.Serialize(vacancyReviewId, _serialiserOptions));
    }
    
    [Test, MoqAutoData]
    public async Task Then_CreateVacancyReviewAsync_Url_Is_Correct(Guid vacancyId, Guid vacancyReviewId)
    {
        // arrange
        var handler = new MockHttpMessageHandler([new HttpResponseMessage(HttpStatusCode.OK)]);
        var sut = CreateSut(handler);

        // act
        await sut.CreateVacancyReviewAsync(vacancyId, vacancyReviewId, AiReviewStatus.Pending, CancellationToken.None);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().Be(new Uri($"http://localhost:8080/ai/vacancies/{vacancyId}/review/{vacancyReviewId}"));
        request.Method.Should().Be(HttpMethod.Post);
        request.Content!.ReadAsStringAsync().Result.Should().Be(JsonSerializer.Serialize(new CreateVacancyReviewData(AiReviewStatus.Pending), _serialiserOptions));
    }

    [Test, MoqAutoData]
    public async Task Then_SendVacancyForManualReviewAsync_Url_Is_Correct(Guid vacancyId, Guid vacancyReviewId)
    {
        // arrange
        var handler = new MockHttpMessageHandler([new HttpResponseMessage(HttpStatusCode.OK)]);
        var sut = CreateSut(handler);

        // act
        await sut.SendVacancyForManualReviewAsync(vacancyId, vacancyReviewId, CancellationToken.None);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().Be(new Uri($"http://localhost:8080/ai/vacancies/{vacancyId}/refer-to-manual"));
        request.Method.Should().Be(HttpMethod.Post);
        
        var content = await request.Content!.ReadAsStringAsync();
        content.Should().Be(JsonSerializer.Serialize(vacancyReviewId, _serialiserOptions));
    }
    
    [Test, MoqAutoData]
    public async Task Then_AutoApproveVacancyAsync_Url_Is_Correct(Guid vacancyId, Guid vacancyReviewId)
    {
        // arrange
        var handler = new MockHttpMessageHandler([new HttpResponseMessage(HttpStatusCode.OK)]);
        var sut = CreateSut(handler);

        // act
        await sut.AutoApproveVacancyAsync(vacancyId, vacancyReviewId, CancellationToken.None);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().Be(new Uri($"http://localhost:8080/ai/vacancies/{vacancyId}/approve"));
        request.Method.Should().Be(HttpMethod.Post);
        
        var content = await request.Content!.ReadAsStringAsync();
        content.Should().Be(JsonSerializer.Serialize(vacancyReviewId, _serialiserOptions));
    }
}