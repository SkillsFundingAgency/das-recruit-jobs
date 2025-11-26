using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.OuterApi.Clients;

namespace SFA.DAS.Recruit.Jobs.UnitTests.OuterApi.Clients.UpdatedPermissionsClientTests;

public class WhenGettingProviderVacanciesToTransfer
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
        long ukprn,
        long accountLegalEntityId,
        List<Guid> vacancies)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(vacancies, options: _serialiserOptions)
        };
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = CreateSut(handler);
        
        // act
        var result = await sut.GetProviderVacanciesToTransfer(ukprn, accountLegalEntityId, CancellationToken.None);

        // assert
        result.Should().BeEquivalentTo(vacancies);
        
        var request = handler.Requests.Single();
        request.RequestUri.Should().Be(new Uri($"http://localhost:8080/updated-employer-permissions/vacancies/transferable?ukprn={ukprn}&accountLegalEntityId={accountLegalEntityId}"));
        request.Method.Should().Be(HttpMethod.Get);
        request.Headers.GetValues("X-Version").Single().Should().Be("1");
    }

    [Test, MoqAutoData]
    public async Task Non_Successful_Responses_Are_Thrown_As_An_Exception(
        long ukprn,
        long accountLegalEntityId)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("Error Content")
        };
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = CreateSut(handler);

        // act
        var action = async () => await sut.GetProviderVacanciesToTransfer(ukprn, accountLegalEntityId, CancellationToken.None);

        // assert
        await action.Should().ThrowAsync<ApiException>();
    }
}