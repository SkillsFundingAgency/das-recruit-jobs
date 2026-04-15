using System.Net;
using AutoFixture.NUnit3;
using SFA.DAS.Recruit.Api.Core.Events;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.EventHandlers;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.AiVacancyReviewing.EventHandlers;

public class WhenHandlingVacancyReviewCreatedEvent
{
    [Test, MoqAutoData]
    public async Task Then_The_Ai_Review_Record_Is_Created_And_The_Message_Queued(
        Mock<IMessageHandlerContext> context,
        [Frozen] Mock<IRecruitAiOuterClient> client,
        [Frozen] Mock<IQueueClient<AiVacancyReviewMessage>> queueClient,
        [Greedy] OnVacancyReviewCreatedEventHandler sut)
    {
        // arrange
        var ev = new VacancyReviewCreatedEvent(Guid.NewGuid(), Guid.NewGuid());
        client
            .Setup(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, context.Object.CancellationToken))
            .ReturnsAsync(new ApiResponse(HttpStatusCode.OK));

        AiVacancyReviewMessage? capturedMessage = null;
        queueClient
            .Setup(x => x.SendMessageAsync(It.IsAny<AiVacancyReviewMessage>()))
            .Callback<AiVacancyReviewMessage>(x => capturedMessage = x)
            .Returns(Task.CompletedTask);

        // act
        await sut.Handle(ev, context.Object);

        // assert
        client.Verify(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, context.Object.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<AiVacancyReviewMessage>()), Times.Once);
        capturedMessage.Should().NotBeNull();
        capturedMessage.VacancyId.Should().Be(ev.VacancyId);
        capturedMessage.VacancyReviewId.Should().Be(ev.VacancyReviewId);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Handler_Captures_Record_Creation_Errors(
        Mock<IMessageHandlerContext> context,
        [Frozen] Mock<IRecruitAiOuterClient> client,
        [Frozen] Mock<IQueueClient<AiVacancyReviewMessage>> queueClient,
        [Greedy] OnVacancyReviewCreatedEventHandler sut)
    {
        // arrange
        var ev = new VacancyReviewCreatedEvent(Guid.NewGuid(), Guid.NewGuid());
        client
            .Setup(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, context.Object.CancellationToken))
            .ReturnsAsync(new ApiResponse(HttpStatusCode.BadRequest));

        // act
        var action = async () => await sut.Handle(ev, context.Object);

        // assert
        await action.Should().ThrowAsync<ApiException>();
    }
}