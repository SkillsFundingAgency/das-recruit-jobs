using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Recruit.Jobs.Core;
using SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Handlers;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Endpoints;

[ExcludeFromCodeCoverage]
public class ArchiveClosedVacanciesTimerTrigger(ILogger<ArchiveClosedVacanciesTimerTrigger> logger,
    IOptions<Core.Configuration.Features> features,
    IArchiveClosedVacanciesHandler handler)
{
    private const string TriggerName = nameof(ArchiveClosedVacanciesTimerTrigger);
    private static readonly TimeSpan ExecutionTimeout = TimeSpan.FromSeconds(240);
    private readonly Core.Configuration.Features _features = features.Value;

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
            // Check if the feature flag to archive vacancies without outcome is enabled. If it is, skip the archiving process.
            // This is to prevent archiving closed vacancies without outcome until the feature flag is disabled, which will be done after the migration of vacancies to archive is complete, we are ready to switch over to the new process.
            if (!_features.ArchiveVacanciesWithoutOutCome)
            {
                logger.LogInformation("[{TriggerName}] Feature flag {FeatureFlag} is enabled. Skipping archiving closed vacancies without outcome.",
                    TriggerName, nameof(Core.Configuration.Features.ArchiveVacanciesWithoutOutCome));
                return;
            }
            
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