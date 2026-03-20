using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Analytics;
using System.Net;
using System.Text.Json;

namespace SFA.DAS.Recruit.Jobs.UnitTests.OuterApi.RecruitJobsOuterClientTests;

[TestFixture]
internal class WhenGettingVacancyAnalyticsByVacancyReference
{
    private readonly JsonSerializerOptions _serialiserOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private RecruitJobsOuterClient CreateSut(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var config = new RecruitJobsOuterApiConfiguration
        {
            BaseUrl = "http://localhost:8080",
            Key = "1234567890"
        };

        return new RecruitJobsOuterClient(httpClient, config, _serialiserOptions);
    }

    [Test, MoqAutoData]
    public async Task Then_The_Request_Is_Correct(long vacancyReference,
        GetOneVacancyAnalyticsResponse response)
    {
        // arrange
        response.VacancyReference = vacancyReference;
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(response, _serialiserOptions))
        };
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = CreateSut(handler);

        // act
        await sut.GetOneVacancyAnalyticsAsync(vacancyReference, CancellationToken.None);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().NotBeNull();
        request.RequestUri.Should().Be(new Uri($"http://localhost:8080/vacancies/{vacancyReference}/analytics"));
        request.Method.Should().Be(HttpMethod.Get);
        request.Headers.GetValues("X-Version").Single().Should().Be("1");
    }

    [Test, MoqAutoData]
    public async Task Then_The_VacanciesAnalytics_Are_Returned_Correctly(long vacancyReference,
        GetOneVacancyAnalyticsResponse response)
    {
        // arrange
        response.VacancyReference = vacancyReference;
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(response, _serialiserOptions))
        };
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = CreateSut(handler);

        // act
        var results = await sut.GetOneVacancyAnalyticsAsync(vacancyReference, CancellationToken.None);

        // assert
        results.Success.Should().BeTrue();
        results.StatusCode.Should().Be(httpResponse.StatusCode);
        results.Payload.Should().BeEquivalentTo(response);
    }
}
