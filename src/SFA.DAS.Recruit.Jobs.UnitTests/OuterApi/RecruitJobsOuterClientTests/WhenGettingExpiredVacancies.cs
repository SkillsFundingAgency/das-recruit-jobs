using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SFA.DAS.Recruit.Jobs.UnitTests.OuterApi.RecruitJobsOuterClientTests;

[TestFixture]
internal class WhenGettingExpiredVacancies
{
    [Test, MoqAutoData]
    public async Task Then_The_Request_Is_Correct(
        DateTime closingDate,
        VacanciesToClose response)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(response, RecruitJobsOuterClientTestExtensions.SerializerOptions))
        };
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = RecruitJobsOuterClientTestExtensions.CreateSut(handler);

        // act
        await sut.GetVacanciesToCloseAsync(closingDate, CancellationToken.None);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().NotBeNull();
        request.RequestUri.Should().Be(new Uri($"http://localhost:8080/vacancies/getVacanciesToClose?pointInTime={UrlEncoder.Default.Encode(closingDate.ToString("s"))}"));
        request.Method.Should().Be(HttpMethod.Get);
        request.Headers.GetValues("X-Version").Single().Should().Be("1");
    }

    [Test, MoqAutoData]
    public async Task Then_The_Vacancies_Are_Returned_Correctly(DateTime closingDate,
        VacanciesToClose response)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(response, RecruitJobsOuterClientTestExtensions.SerializerOptions))
        };
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = RecruitJobsOuterClientTestExtensions.CreateSut(handler);

        // act
        var results = await sut.GetVacanciesToCloseAsync(closingDate, CancellationToken.None);

        // assert
        results.Success.Should().BeTrue();
        results.StatusCode.Should().Be(httpResponse.StatusCode);
        results.Payload.Should().BeEquivalentTo(response);
    }
}
