using System.Net;
using System.Text.Json;

namespace SFA.DAS.Recruit.Jobs.UnitTests.OuterApi.RecruitJobsOuterClientTests;

public class WhenDeletingDelayedNotifications
{
    [Test, MoqAutoData]
    public async Task Then_The_Request_Is_Correct(List<long> ids)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = RecruitJobsOuterClientTestExtensions.CreateSut(handler);
        
        // act
        await sut.DeleteDelayedNotificationsAsync(ids);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().Be(new Uri("http://localhost:8080/delayed-notifications/delete"));
        request.Method.Should().Be(HttpMethod.Post);
        request.Headers.GetValues("X-Version").Single().Should().Be("1");
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Ids_Are_Sent_As_The_Http_Content(List<long> ids)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
        var handler = new MockHttpMessageHandler([httpResponse]);
        var expectedContent = JsonSerializer.Serialize(ids, RecruitJobsOuterClientTestExtensions.SerializerOptions);
        var sut = RecruitJobsOuterClientTestExtensions.CreateSut(handler);
        
        // act
        await sut.DeleteDelayedNotificationsAsync(ids);

        // assert
        var content = await handler.Requests.Single().Content!.ReadAsStringAsync();
        content.Should().Be(expectedContent);
    }
}