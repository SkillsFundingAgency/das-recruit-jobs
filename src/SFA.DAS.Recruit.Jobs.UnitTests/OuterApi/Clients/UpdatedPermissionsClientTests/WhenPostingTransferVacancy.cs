using System.Net;
using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi.Clients;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.UnitTests.OuterApi.Clients.UpdatedPermissionsClientTests;

public class WhenPostingTransferVacancy
{
    private readonly JsonSerializerOptions _serialiserOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private UpdatedPermissionsClient CreateSut(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var config = new RecruitJobsOuterApiConfiguration
        {
            BaseUrl = "http://localhost:8080",
            Key = "1234567890"
        };

        return new UpdatedPermissionsClient(httpClient, config, _serialiserOptions);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Legal_Entity_Public_Hash_Id_Should_Be_Returned(
        Guid vacancyId,
        TransferReason transferReason)
    {
        // arrange
        var expectedContent = JsonSerializer.Serialize(new TransferVacancyRequest(transferReason), _serialiserOptions);
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = CreateSut(handler);
        
        // act
        await sut.TransferVacancyAsync(vacancyId, transferReason, CancellationToken.None);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().Be(new Uri($"http://localhost:8080/updated-employer-permissions/vacancies/{vacancyId}/transfer"));
        request.Method.Should().Be(HttpMethod.Post);
        request.Headers.GetValues("X-Version").Single().Should().Be("1");
        (await request.Content!.ReadAsStringAsync()).Should().Be(expectedContent);
    }

    [Test, MoqAutoData]
    public async Task Non_Successful_Responses_Are_Thrown_As_An_Exception(
        Guid vacancyId,
        TransferReason transferReason)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("Error Content")
        };
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = CreateSut(handler);

        // act
        var action = async () => await sut.TransferVacancyAsync(vacancyId, transferReason, CancellationToken.None);

        // assert
        await action.Should().ThrowAsync<ApiException>();
    }
}