using System.Diagnostics.CodeAnalysis;
using System.Net;
using AutoFixture.NUnit3;
using Microsoft.FeatureManagement;
using SFA.DAS.Recruit.Api.Core.Events;
using SFA.DAS.Recruit.Jobs.Core.Configuration;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.EventHandlers;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.AiVacancyReviewing.EventHandlers;

[SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly")]
public class WhenHandlingVacancyReviewCreatedEvent
{
    [Test, MoqAutoData]
    public async Task Then_The_Ai_Review_Record_Is_Created_And_The_Message_Queued_When_The_Feature_Is_Enabled(
        Mock<IMessageHandlerContext> context,
        [Frozen] Mock<IVariantFeatureManager> featureManager,
        [Frozen] Mock<IRecruitAiOuterClient> client,
        [Frozen] Mock<IQueueClient<AiVacancyReviewMessage>> queueClient,
        [Greedy] OnVacancyReviewCreatedEventHandler sut)
    {
        // arrange
        featureManager
            .Setup(x => x.IsEnabledAsync(FeatureFlags.AiReviews, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(true));
        var ev = new VacancyReviewCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), false, true);
        client
            .Setup(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, AiReviewStatus.Pending, context.Object.CancellationToken))
            .ReturnsAsync(new ApiResponse(true, HttpStatusCode.OK));

        AiVacancyReviewMessage? capturedMessage = null;
        queueClient
            .Setup(x => x.SendMessageAsync(It.IsAny<AiVacancyReviewMessage>()))
            .Callback<AiVacancyReviewMessage>(x => capturedMessage = x)
            .Returns(Task.CompletedTask);

        // act
        await sut.Handle(ev, context.Object);

        // assert
        client.Verify(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, AiReviewStatus.Pending, context.Object.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<AiVacancyReviewMessage>()), Times.Once);
        capturedMessage.Should().NotBeNull();
        capturedMessage.VacancyId.Should().Be(ev.VacancyId);
        capturedMessage.VacancyReviewId.Should().Be(ev.VacancyReviewId);
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_The_Vacancy_Is_A_Resubmission_The_Ai_Review_Record_Is_Created_And_The_Message_Queued(
        Mock<IMessageHandlerContext> context,
        [Frozen] Mock<IVariantFeatureManager> featureManager,
        [Frozen] Mock<IRecruitAiOuterClient> client,
        [Frozen] Mock<IQueueClient<AiVacancyReviewMessage>> queueClient,
        [Greedy] OnVacancyReviewCreatedEventHandler sut)
    {
        // arrange
        featureManager
            .Setup(x => x.IsEnabledAsync(FeatureFlags.AiReviews, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(true));
        var ev = new VacancyReviewCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), true, true);
        client
            .Setup(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, AiReviewStatus.Skipped, context.Object.CancellationToken))
            .ReturnsAsync(new ApiResponse(true, HttpStatusCode.OK));

        // act
        await sut.Handle(ev, context.Object);

        // assert
        client.Verify(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, AiReviewStatus.Skipped, context.Object.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<AiVacancyReviewMessage>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_The_Vacancy_Failed_Auto_Qa_Checks_The_Ai_Review_Record_Is_Created_And_The_Message_Queued(
        Mock<IMessageHandlerContext> context,
        [Frozen] Mock<IVariantFeatureManager> featureManager,
        [Frozen] Mock<IRecruitAiOuterClient> client,
        [Frozen] Mock<IQueueClient<AiVacancyReviewMessage>> queueClient,
        [Greedy] OnVacancyReviewCreatedEventHandler sut)
    {
        // arrange
        featureManager
            .Setup(x => x.IsEnabledAsync(FeatureFlags.AiReviews, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(true));
        var ev = new VacancyReviewCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), false, false);
        client
            .Setup(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, AiReviewStatus.Skipped, context.Object.CancellationToken))
            .ReturnsAsync(new ApiResponse(true, HttpStatusCode.OK));

        // act
        await sut.Handle(ev, context.Object);

        // assert
        client.Verify(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, AiReviewStatus.Skipped, context.Object.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<AiVacancyReviewMessage>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_The_Feature_Is_Disabled_The_Ai_Review_Record_Is_Created_And_The_Message_Queued_With_A_Status_Of_Skipped(
        Mock<IMessageHandlerContext> context,
        [Frozen] Mock<IVariantFeatureManager> featureManager,
        [Frozen] Mock<IRecruitAiOuterClient> client,
        [Frozen] Mock<IQueueClient<AiVacancyReviewMessage>> queueClient,
        [Greedy] OnVacancyReviewCreatedEventHandler sut)
    {
        // arrange
        featureManager
            .Setup(x => x.IsEnabledAsync(FeatureFlags.AiReviews, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(false));
        var ev = new VacancyReviewCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), true, true);
        client
            .Setup(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, AiReviewStatus.Skipped, context.Object.CancellationToken))
            .ReturnsAsync(new ApiResponse(true, HttpStatusCode.OK));

        // act
        await sut.Handle(ev, context.Object);

        // assert
        client.Verify(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, AiReviewStatus.Skipped, context.Object.CancellationToken), Times.Once);
        queueClient.Verify(x => x.SendMessageAsync(It.IsAny<AiVacancyReviewMessage>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Handler_Captures_Record_Creation_Errors(
        Mock<IMessageHandlerContext> context,
        [Frozen] Mock<IVariantFeatureManager> featureManager,
        [Frozen] Mock<IRecruitAiOuterClient> client,
        [Frozen] Mock<IQueueClient<AiVacancyReviewMessage>> queueClient,
        [Greedy] OnVacancyReviewCreatedEventHandler sut)
    {
        // arrange
        featureManager
            .Setup(x => x.IsEnabledAsync(FeatureFlags.AiReviews, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(true));
        var ev = new VacancyReviewCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), false, true);
        client
            .Setup(x => x.CreateVacancyReviewAsync(ev.VacancyId, ev.VacancyReviewId, AiReviewStatus.Pending, context.Object.CancellationToken))
            .ReturnsAsync(new ApiResponse(false, HttpStatusCode.BadRequest));

        // act
        var action = async () => await sut.Handle(ev, context.Object);

        // assert
        await action.Should().ThrowAsync<ApiException>();
    }
}