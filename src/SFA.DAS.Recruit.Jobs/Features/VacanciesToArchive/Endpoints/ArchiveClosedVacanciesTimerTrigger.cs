using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Handlers;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Recruit.Jobs.Core;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Endpoints;

[ExcludeFromCodeCoverage]
public class ArchiveClosedVacanciesTimerTrigger(ILogger<ArchiveClosedVacanciesTimerTrigger> logger,
    IArchiveClosedVacanciesHandler handler)
{
    private const string TriggerName = nameof(ArchiveClosedVacanciesTimerTrigger);
    private static readonly TimeSpan ExecutionTimeout = TimeSpan.FromSeconds(240);

    // Timer set to run every 6:00 AM daily
    [Function(TriggerName)]
    public async Task Run([TimerTrigger(Schedules.SixAmDaily)] TimerInfo _,
        FunctionContext context,
        CancellationToken cancellationToken)
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