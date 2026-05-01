using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Endpoints;

[ExcludeFromCodeCoverage]
public class ArchiveClosedVacanciesMigrationTimerTrigger(ILogger<ArchiveClosedVacanciesMigrationTimerTrigger> logger,
    IOptions<Core.Configuration.Features> features,
    VacancyArchivingStrategy vacancyArchivingStrategy)
{
    private const string TriggerName = nameof(ArchiveClosedVacanciesMigrationTimerTrigger);
    private static readonly TimeSpan ExecutionTimeout = TimeSpan.FromSeconds(240);
    private readonly Core.Configuration.Features _features = features.Value;


    public async Task Run([TimerTrigger("* 3 * * *")] TimerInfo timerInfo, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(ExecutionTimeout);
        try
        {
            // Check if the feature flag to archive vacancies without outcome is enabled. If it is, skip the archiving process.
            // This is to prevent archiving closed vacancies without outcome until the feature flag is disabled, which will be done after the migration of vacancies to archive is complete, we are ready to switch over to the new process.
            if (_features.ArchiveVacanciesWithoutOutCome)
            {
                logger.LogInformation("[{TriggerName}] Feature flag {FeatureFlag} is enabled. Skipping archiving closed vacancies.",
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