using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Features.Reports.Handlers;
using SFA.DAS.Recruit.Jobs.NServiceBus.Events;

namespace SFA.DAS.Recruit.Jobs.Features.Reports.EventHandlers;

public class OnReportCreatedEventHandler(ILogger<OnReportCreatedEventHandler> logger,IReportCreatedHandler reportCreatedHandler) : IHandleMessages<ReportCreatedEvent>
{
    public async Task Handle(ReportCreatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Handling ReportCreatedEvent for report {reportId}", message.ReportId);
        await reportCreatedHandler.RunAsync(message.ReportId, context.CancellationToken);
    }
}