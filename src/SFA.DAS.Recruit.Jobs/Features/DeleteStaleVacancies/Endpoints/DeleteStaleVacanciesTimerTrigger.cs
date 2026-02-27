using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core.Models;
using SFA.DAS.Recruit.Jobs.Features.DeleteStaleVacancies.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.DeleteStaleVacancies.Endpoints;

[ExcludeFromCodeCoverage]
public class DeleteStaleVacanciesTimerTrigger(ILogger<DeleteStaleVacanciesTimerTrigger> logger,
    IDeleteStaleVacanciesHandler handler)
{
    private const string TriggerName = nameof(DeleteStaleVacanciesTimerTrigger);
    private static readonly TimeSpan ExecutionTimeout = TimeSpan.FromSeconds(240);

    // Timer set to run at 7:00 AM every Sunday
    [Function(TriggerName)]
    public async Task Run([TimerTrigger(Schedules.WeeklySevenAmSunday)] TimerInfo _, CancellationToken cancellationToken)
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