using System.Net;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Events;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.EventHandlers;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.AiVacancyReviewing.EventHandlers;

public class WhenHandlingAiVacancyReviewCompletedEvent
{
    [Test, MoqAutoData]
    public async Task Then_The_Handler_Auto_Approves_The_Vacancy(
        Mock<IMessageHandlerContext> context,
        [Frozen] Mock<IRecruitAiOuterClient> client,
        [Greedy] OnAiVacancyReviewCompletedEventHandler sut)
    {
        // arrange
        var ev = new AiVacancyReviewCompletedEvent(Guid.NewGuid(), Guid.NewGuid(), AiReviewStatus.Passed, false);
        client
            .Setup(x => x.AutoApproveVacancyAsync(ev.VacancyId, ev.VacancyReviewId, context.Object.CancellationToken))
            .ReturnsAsync(new ApiResponse(HttpStatusCode.OK));

        // act
        await sut.Handle(ev, context.Object);

        // assert
        client.Verify(x => x.AutoApproveVacancyAsync(ev.VacancyId, ev.VacancyReviewId, context.Object.CancellationToken), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Handler_Captures_Approve_Failures(
        Mock<IMessageHandlerContext> context,
        [Frozen] Mock<IRecruitAiOuterClient> client,
        [Greedy] OnAiVacancyReviewCompletedEventHandler sut)
    {
        // arrange
        var ev = new AiVacancyReviewCompletedEvent(Guid.NewGuid(), Guid.NewGuid(), AiReviewStatus.Passed, false);
        client
            .Setup(x => x.AutoApproveVacancyAsync(ev.VacancyId, ev.VacancyReviewId, context.Object.CancellationToken))
            .ReturnsAsync(new ApiResponse(HttpStatusCode.BadRequest));

        // act
        var action = async () => await sut.Handle(ev, context.Object);

        // assert
        await action.Should().ThrowAsync<ApiException>();
    }
    
    [Test]
    [MoqInlineAutoData(AiReviewStatus.Failed, false)]
    [MoqInlineAutoData(AiReviewStatus.Failed, true)]
    [MoqInlineAutoData(AiReviewStatus.Pending, false)]
    [MoqInlineAutoData(AiReviewStatus.Pending, true)]
    public async Task Then_The_Handler_Refers_The_Vacancy_For_Manual_Review(
        AiReviewStatus status,
        bool flaggedForReview,
        Mock<IMessageHandlerContext> context,
        [Frozen] Mock<IRecruitAiOuterClient> client,
        [Greedy] OnAiVacancyReviewCompletedEventHandler sut)
    {
        // arrange
        var ev = new AiVacancyReviewCompletedEvent(Guid.NewGuid(), Guid.NewGuid(), status, flaggedForReview);
        client
            .Setup(x => x.SendVacancyForManualReviewAsync(ev.VacancyId, ev.VacancyReviewId, context.Object.CancellationToken))
            .ReturnsAsync(new ApiResponse(HttpStatusCode.OK));

        // act
        await sut.Handle(ev, context.Object);

        // assert
        client.Verify(x => x.SendVacancyForManualReviewAsync(ev.VacancyId, ev.VacancyReviewId, context.Object.CancellationToken), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Handler_Captures_Refer_Failures(
        Mock<IMessageHandlerContext> context,
        [Frozen] Mock<IRecruitAiOuterClient> client,
        [Greedy] OnAiVacancyReviewCompletedEventHandler sut)
    {
        // arrange
        var ev = new AiVacancyReviewCompletedEvent(Guid.NewGuid(), Guid.NewGuid(), AiReviewStatus.Failed, true);
        client
            .Setup(x => x.SendVacancyForManualReviewAsync(ev.VacancyId, ev.VacancyReviewId, context.Object.CancellationToken))
            .ReturnsAsync(new ApiResponse(HttpStatusCode.BadRequest));

        // act
        var action = async () => await sut.Handle(ev, context.Object);

        // assert
        await action.Should().ThrowAsync<ApiException>();
    }
}