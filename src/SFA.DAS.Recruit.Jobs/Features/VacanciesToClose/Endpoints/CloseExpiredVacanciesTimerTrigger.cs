using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Features.VacanciesToClose.Handlers;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToClose.Endpoints;

[ExcludeFromCodeCoverage]
public class CloseExpiredVacanciesTimerTrigger(
    ILogger<CloseExpiredVacanciesTimerTrigger> logger,
    ICloseExpiredVacanciesHandler handler)
{
    private const string TriggerName = nameof(CloseExpiredVacanciesTimerTrigger);
    private static readonly TimeSpan ExecutionTimeout = TimeSpan.FromSeconds(240);
    private const string DailySchedule = "0 0 0 * * *";

    // Timer set to run at 00:00 AM every day
    [Function(TriggerName)]
    public async Task Run([TimerTrigger(DailySchedule)] TimerInfo _, CancellationToken cancellationToken)
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
            logger.LogError(ex, "[{TriggerName}] Unhandled exception occurred while running cleanup job", TriggerName);
        }
        finally
        {
            logger.LogInformation("[{TriggerName}] Timer trigger finished at {CompletedUtc}", TriggerName, DateTime.UtcNow);
        }
    }
}