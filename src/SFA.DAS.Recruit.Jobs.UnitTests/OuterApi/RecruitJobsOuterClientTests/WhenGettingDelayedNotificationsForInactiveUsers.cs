using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using System.Net;
using System.Text.Json;

namespace SFA.DAS.Recruit.Jobs.UnitTests.OuterApi.RecruitJobsOuterClientTests;

public class WhenGettingDelayedNotificationsForInactiveUsers
{
    [Test, MoqAutoData]
    public async Task Then_The_Request_Is_Correct(
        List<NotificationEmail> emails)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(emails, RecruitJobsOuterClientTestExtensions.SerializerOptions))
        };
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = RecruitJobsOuterClientTestExtensions.CreateSut(handler);

        // act
        await sut.GetDelayedNotificationsBatchByUsersInactiveStatus(CancellationToken.None);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().Be(new Uri("http://localhost:8080/delayed-notifications/users/inactive"));
        request.Method.Should().Be(HttpMethod.Get);
        request.Headers.GetValues("X-Version").Single().Should().Be("1");
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Emails_Are_Returned_Correctly(List<NotificationEmail> emails)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(emails, RecruitJobsOuterClientTestExtensions.SerializerOptions))
        };
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = RecruitJobsOuterClientTestExtensions.CreateSut(handler);

        // act
        var results = await sut.GetDelayedNotificationsBatchByUsersInactiveStatus(CancellationToken.None);

        // assert
        results.Success.Should().BeTrue();
        results.StatusCode.Should().Be(httpResponse.StatusCode);
        results.Payload.Should().BeEquivalentTo(emails);
    }
}