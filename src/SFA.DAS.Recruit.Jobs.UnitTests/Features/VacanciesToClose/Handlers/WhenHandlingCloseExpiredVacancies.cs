using AutoFixture.NUnit3;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Features.VacanciesToClose.Handlers;
using SFA.DAS.Recruit.Jobs.OuterApi;
using System.Net;
using Esfa.Recruit.Vacancies.Client.Domain.Events;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.VacanciesToClose.Handlers;

[TestFixture]
internal class WhenHandlingCloseExpiredVacancies
{
    [Test, MoqAutoData]
    public async Task RunAsync_Should_Publish_Event_For_ExpiredVacancies_WhenResponseIsValid(
        Jobs.OuterApi.Common.VacanciesToClose response,
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] CloseExpiredVacanciesHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacanciesToCloseAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToClose>(true, HttpStatusCode.OK, response, null));

        // Act
        await sut.RunAsync(context.Object, CancellationToken.None);

        // Assert
        functionEndpoint.Verify(
            x => x.Send(It.IsAny<VacancyClosedEvent>(),
                It.IsAny<SendOptions>(),
                It.IsAny<FunctionContext>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(response.Data.Count()));
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseIsNull(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] CloseExpiredVacanciesHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacanciesToCloseAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToClose>(true, HttpStatusCode.OK, new Jobs.OuterApi.Common.VacanciesToClose()));

        // Act
        await sut.RunAsync(context.Object, CancellationToken.None);

        // Assert
        functionEndpoint.Verify(
            x => x.Send(It.IsAny<VacancyClosedEvent>(),
                It.IsAny<SendOptions>(),
                It.IsAny<FunctionContext>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_GetResponseFails(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] CloseExpiredVacanciesHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacanciesToCloseAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToClose>(false, HttpStatusCode.InternalServerError, new Jobs.OuterApi.Common.VacanciesToClose(), "Internal error"));

        // Act
        await sut.RunAsync(context.Object, CancellationToken.None);

        // Assert
        functionEndpoint.Verify(
            x => x.Send(It.IsAny<VacancyClosedEvent>(),
                It.IsAny<SendOptions>(),
                It.IsAny<FunctionContext>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task RunAsync_Should_Return_When_NoVacanciesFound(
        [Frozen] Mock<IRecruitJobsOuterClient> jobsOuterClient,
        [Frozen] Mock<IFunctionEndpoint> functionEndpoint,
        [Frozen] Mock<FunctionContext> context,
        [Greedy] CloseExpiredVacanciesHandler sut)
    {
        // Arrange
        jobsOuterClient
            .Setup(x => x.GetVacanciesToCloseAsync(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new ApiResponse<Jobs.OuterApi.Common.VacanciesToClose>(true, HttpStatusCode.OK, new(), null));

        // Act
        await sut.RunAsync(context.Object, CancellationToken.None);

        // Assert
        functionEndpoint.Verify(
            x => x.Send(It.IsAny<VacancyClosedEvent>(),
                It.IsAny<SendOptions>(),
                It.IsAny<FunctionContext>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }
}
