using System.Net;
using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Core.Http.ClientBaseTests;

public class WhenPosting
{
    private readonly JsonSerializerOptions _serialiserOptions = new();
    
    private TestClientBase CreateSut(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
        return new TestClientBase(httpClient, _serialiserOptions);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Request_Is_Made_Correctly()
    {
        // arrange
        var handler = new MockHttpMessageHandler([new HttpResponseMessage(HttpStatusCode.NoContent)]);
        var sut = CreateSut(handler);
        
        // act
        await sut.Post<NoResponse>("/api/foo");

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().Be(new Uri("http://localhost:8080/api/foo"));
        request.Method.Should().Be(HttpMethod.Post);
        request.Headers.GetValues("X-Version").Single().Should().Be("1.0");
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Request_Content_Is_Correct(NotificationEmail email)
    {
        // arrange
        var handler = new MockHttpMessageHandler([new HttpResponseMessage(HttpStatusCode.NoContent)]);
        var sut = CreateSut(handler);
        var expectedContent = JsonSerializer.Serialize(email, _serialiserOptions);
        
        // act
        await sut.Post<NoResponse>("/api/foo", email);

        // assert
        var request = handler.Requests.Single();
        var content = await request.Content.ReadAsStringAsync();
        content.Should().Be(expectedContent);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Response_Is_Processed_Correctly(NotificationEmail email)
    {
        // arrange
        var response = new HttpResponseMessage(HttpStatusCode.NoContent)
        {
            Content = new StringContent(JsonSerializer.Serialize(email, _serialiserOptions))
        };
        var handler = new MockHttpMessageHandler([response]);
        var sut = CreateSut(handler);
        
        // act
        var result = await sut.Post<NotificationEmail>("/api/foo");

        // assert
        result.StatusCode.Should().Be(response.StatusCode);
        result.Payload.Should().BeEquivalentTo(email);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Error_Content_Is_Returned_Correctly(NotificationEmail email)
    {
        // arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("foo")
        };
        var handler = new MockHttpMessageHandler([response]);
        var sut = CreateSut(handler);
        
        // act
        var result = await sut.Post<NotificationEmail>("/api/foo");

        // assert
        result.StatusCode.Should().Be(response.StatusCode);
        result.Payload.Should().Be(null);
        result.ErrorContent.Should().Be("foo");
    }
}