using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Endpoints;

[ExcludeFromCodeCoverage]
public class DelayedNotificationsTimerTrigger(
    ILogger<DelayedNotificationsTimerTrigger> logger,
    IDelayedNotificationsEnqueueHandler handler)
{
    private const string TriggerName = nameof(DelayedNotificationsTimerTrigger);
    private const int ExecutionDurationInSeconds = 240;
    
    [Function(TriggerName)]
    public async Task Run([TimerTrigger("0/5 * * * *", RunOnStartup = true)] TimerInfo timerInfo, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(ExecutionDurationInSeconds));
            await handler.RunAsync(cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured whilst enqueuing emails", TriggerName);
        }
        finally
        {
            logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
        }
    }
}