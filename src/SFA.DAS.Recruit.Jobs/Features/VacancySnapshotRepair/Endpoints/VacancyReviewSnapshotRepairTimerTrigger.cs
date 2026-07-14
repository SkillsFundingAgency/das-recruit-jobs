using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacancySnapshotRepair.Endpoints;

[ExcludeFromCodeCoverage]
public class VacancyReviewSnapshotRepairTimerTrigger(ILogger<VacancyReviewSnapshotRepairTimerTrigger> logger,
    VacancyReviewSnapshotRepairStrategy vacancyReviewSnapshotRepairStrategy)
{
    private const string TriggerName = nameof(VacancyReviewSnapshotRepairTimerTrigger);
    private static readonly TimeSpan ExecutionTimeout = TimeSpan.FromSeconds(240);

    [Function(TriggerName)]
    public async Task Run([TimerTrigger(Schedules.EveryFifteenMinutes)] TimerInfo timerInfo, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(ExecutionTimeout);
        try
        {
            await vacancyReviewSnapshotRepairStrategy.RunAsync(linkedCts.Token);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during migration", TriggerName);
            throw;
        }
        logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
    }
}
