using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Recruit.Jobs.Core;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Endpoints;

[ExcludeFromCodeCoverage]
public class ArchiveClosedVacanciesMigrationTimerTrigger(ILogger<ArchiveClosedVacanciesMigrationTimerTrigger> logger,
    IOptions<Core.Configuration.Features> features,
    VacancyArchivingStrategy vacancyArchivingStrategy)
{
    private const string TriggerName = nameof(ArchiveClosedVacanciesMigrationTimerTrigger);
    private static readonly TimeSpan ExecutionTimeout = TimeSpan.FromSeconds(240);
    private readonly Core.Configuration.Features _features = features.Value;

    [Function(TriggerName)]
    public async Task Run([TimerTrigger(Schedules.EveryFifteenMinutes)] TimerInfo timerInfo, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(ExecutionTimeout);
        try
        {
            // Check if the feature flag to archive vacancies without outcome is disabled. If it is, skip the archiving process.
            // This is to prevent archiving closed vacancies without outcome until the feature flag is enabled, which will be done after the migration of vacancies to archive is complete, we are ready to switch over to the new process.
            if (!_features.ArchiveVacanciesWithoutOutCome)
            {
                logger.LogInformation("[{TriggerName}] Feature flag {FeatureFlag} is disabled. Skipping archiving closed vacancies.",
                    TriggerName, nameof(Core.Configuration.Features.ArchiveVacanciesWithoutOutCome));
                return;
            }

            await vacancyArchivingStrategy.RunAsync(linkedCts.Token);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during migration", TriggerName);
            throw;
        }
        logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
    }
}