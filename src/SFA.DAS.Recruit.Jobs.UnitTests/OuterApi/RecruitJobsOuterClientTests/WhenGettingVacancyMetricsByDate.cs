using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy.Metrics;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SFA.DAS.Recruit.Jobs.UnitTests.OuterApi.RecruitJobsOuterClientTests;

[TestFixture]
internal class WhenGettingVacancyMetricsByDate
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
    public async Task Then_The_Request_Is_Correct(
        long vacancyReference,
        DateTime startDate,
        DateTime endDate,
        VacancyMetricResponse response)
    {
        // arrange
        foreach (var metric in response.VacancyMetrics)
        {
            metric.VacancyReference = vacancyReference.ToString();
        }
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(response, _serialiserOptions))
        };
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = CreateSut(handler);

        // act
        await sut.GetVacancyMetricsByDateAsync(startDate, endDate, CancellationToken.None);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().NotBeNull();
        request.RequestUri.Should().Be(new Uri($"http://localhost:8080/metrics/vacancies?startDate={UrlEncoder.Default.Encode(startDate.ToString("s"))}&endDate={UrlEncoder.Default.Encode(endDate.ToString("s"))}"));
        request.Method.Should().Be(HttpMethod.Get);
        request.Headers.GetValues("X-Version").Single().Should().Be("1");
    }

    [Test, MoqAutoData]
    public async Task Then_The_Vacancies_Are_Returned_Correctly(long vacancyReference, 
        DateTime startDate,
        DateTime endDate,
        VacancyMetricResponse response)
    {
        // arrange
        foreach (var metric in response.VacancyMetrics)
        {
            metric.VacancyReference = vacancyReference.ToString();
        }
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(response, _serialiserOptions))
        };
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = CreateSut(handler);

        // act
        var results = await sut.GetVacancyMetricsByDateAsync(startDate, endDate, CancellationToken.None);

        // assert
        results.Success.Should().BeTrue();
        results.StatusCode.Should().Be(httpResponse.StatusCode);
        results.Payload.Should().BeEquivalentTo(response);
    }
}