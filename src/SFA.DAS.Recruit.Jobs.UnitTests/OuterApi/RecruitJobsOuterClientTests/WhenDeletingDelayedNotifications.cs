using System.Net;
using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.OuterApi;

namespace SFA.DAS.Recruit.Jobs.UnitTests.OuterApi.RecruitJobsOuterClientTests;

public class WhenDeletingDelayedNotifications
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
    public async Task Then_The_Request_Is_Correct(List<long> ids)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = CreateSut(handler);
        
        // act
        await sut.DeleteDelayedNotificationsAsync(ids);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().Be(new Uri("http://localhost:8080/delayed-notifications/delete"));
        request.Method.Should().Be(HttpMethod.Post);
        request.Headers.GetValues("X-Version").Single().Should().Be("1.0");
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Ids_Are_Sent_As_The_Http_Content(List<long> ids)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
        var handler = new MockHttpMessageHandler([httpResponse]);
        var expectedContent = JsonSerializer.Serialize(ids, _serialiserOptions);
        var sut = CreateSut(handler);
        
        // act
        await sut.DeleteDelayedNotificationsAsync(ids);

        // assert
        var content = await handler.Requests.Single().Content!.ReadAsStringAsync();
        content.Should().Be(expectedContent);
    }
}