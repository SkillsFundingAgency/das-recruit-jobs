using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Models;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Analytics;
using System.Net;
using System.Text.Json;

namespace SFA.DAS.Recruit.Jobs.UnitTests.OuterApi.RecruitJobsOuterClientTests;

[TestFixture]
internal class WhenPuttingVacancyAnalyticsByVacancyReference
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
        List<VacancyAnalytics> vacancyAnalytics)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = CreateSut(handler);

        // act
        await sut.PutOneVacancyAnalyticsAsync(vacancyReference, vacancyAnalytics, CancellationToken.None);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().NotBeNull();
        request.RequestUri.Should().Be(new Uri($"http://localhost:8080/vacancies/{vacancyReference}/analytics"));
        request.Method.Should().Be(HttpMethod.Put);
        request.Headers.GetValues("X-Version").Single().Should().Be("1");
    }

    [Test, MoqAutoData]
    public async Task Then_The_VacanciesAnalytics_Data_Are_Returned_Correctly(long vacancyReference,
        List<VacancyAnalytics> vacancyAnalytics)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
        var handler = new MockHttpMessageHandler([httpResponse]);
        var expectedContent = JsonSerializer.Serialize(new PutOneVacancyAnalyticsRequest(vacancyAnalytics), _serialiserOptions);
        var sut = CreateSut(handler);

        // act
        await sut.PutOneVacancyAnalyticsAsync(vacancyReference, vacancyAnalytics, CancellationToken.None);

        // assert
        var content = await handler.Requests.Single().Content!.ReadAsStringAsync();
        content.Should().Be(expectedContent);
    }
}