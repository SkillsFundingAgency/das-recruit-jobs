using SFA.DAS.Recruit.Jobs.Features.Reports.EventHandlers;
using SFA.DAS.Recruit.Jobs.Features.Reports.Handlers;
using SFA.DAS.Recruit.Jobs.NServiceBus.Events;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Features.GenerateReportHandling.EventHandlers;

public class WhenHandlingReportCreatedEvent
{
    [Test, MoqAutoData]
    public async Task Then_The_ReportCreatedEvent_Is_Handled(
        Guid id,
        IMessageHandlerContext context,
        [Frozen] Mock<IReportCreatedHandler> handler,
        [Greedy] OnReportCreatedEventHandler sut)
    {
        // act
        await sut.Handle(new ReportCreatedEvent(id), context);

        // assert
        handler.Verify(x => x.RunAsync(id, context.CancellationToken), Times.Once);
    }
}