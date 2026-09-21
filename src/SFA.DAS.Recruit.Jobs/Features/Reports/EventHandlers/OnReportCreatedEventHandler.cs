using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Api.Core.Events;
using SFA.DAS.Recruit.Jobs.Features.Reports.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.Reports.EventHandlers;

public class OnReportCreatedEventHandler(ILogger<OnReportCreatedEventHandler> logger,IReportCreatedHandler reportCreatedHandler) : IHandleMessages<ReportCreatedEvent>
{
    public async Task Handle(ReportCreatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Handling ReportCreatedEvent for report {reportId}", message.ReportId);
        await reportCreatedHandler.RunAsync(message.ReportId, context.CancellationToken);
    }
}