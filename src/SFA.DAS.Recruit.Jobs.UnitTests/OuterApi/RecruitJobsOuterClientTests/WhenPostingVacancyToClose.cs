using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using System.Net;

namespace SFA.DAS.Recruit.Jobs.UnitTests.OuterApi.RecruitJobsOuterClientTests;

[TestFixture]
internal class WhenPostingVacancyToClose
{
    [Test, MoqAutoData]
    public async Task Then_The_Request_Is_Correct(Guid vacancyId, long vacancyReference,
        ClosureReason closureReason)
    {
        // arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
        var handler = new MockHttpMessageHandler([httpResponse]);
        var sut = RecruitJobsOuterClientTestExtensions.CreateSut(handler);

        // act
        await sut.PostVacancyToClose(vacancyId, vacancyReference, closureReason, CancellationToken.None);

        // assert
        var request = handler.Requests.Single();
        request.RequestUri.Should().NotBeNull();
        request.RequestUri.Should().Be(new Uri($"http://localhost:8080/vacancies/{vacancyReference}/close"));
        request.Method.Should().Be(HttpMethod.Post);
        request.Headers.GetValues("X-Version").Single().Should().Be("1");
    }
}