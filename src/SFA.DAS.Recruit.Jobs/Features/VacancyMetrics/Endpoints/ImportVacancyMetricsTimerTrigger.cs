using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Features.VacancyMetrics.Handlers;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Recruit.Jobs.Core;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMetrics.Endpoints;

[ExcludeFromCodeCoverage]
public class ImportVacancyMetricsTimerTrigger(ILogger<ImportVacancyMetricsTimerTrigger> logger,
    IImportVacancyMetricsHandler handler)
{
    private const string TriggerName = nameof(ImportVacancyMetricsTimerTrigger);
    private static readonly TimeSpan ExecutionTimeout = TimeSpan.FromSeconds(240);

    // Timer set to run at every hour
    [Function(TriggerName)]
    public async Task Run([TimerTrigger(Schedules.Hourly)] TimerInfo _, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{TriggerName}] Timer trigger fired at {NowUtc}", TriggerName, DateTime.UtcNow);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(ExecutionTimeout);

        try
        {
            await handler.RunAsync(linkedCts.Token);
            logger.LogInformation("[{TriggerName}] Successfully completed execution", TriggerName);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[{TriggerName}] Execution cancelled after timeout of {TimeoutSeconds} seconds",
                TriggerName, ExecutionTimeout.TotalSeconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{TriggerName}] Unhandled exception occurred while running job", TriggerName);
        }
        finally
        {
            logger.LogInformation("[{TriggerName}] Timer trigger finished at {CompletedUtc}", TriggerName, DateTime.UtcNow);
        }
    }
}
