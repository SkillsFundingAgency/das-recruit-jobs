using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.QaReports;

[ExcludeFromCodeCoverage]
public class QaReportsMigrationTimerTrigger(ILogger<QaReportsMigrationTimerTrigger> logger, QaReportsMigrationStrategy qaReportsMigrationStrategy)
{
    private const string TriggerName = nameof(QaReportsMigrationTimerTrigger);

    [Function(TriggerName)]
    public async Task Run([TimerTrigger("*/10 * * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await qaReportsMigrationStrategy.RunAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during migration", TriggerName);
            throw;
        }
        logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
    }
}
