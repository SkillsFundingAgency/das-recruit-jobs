using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyAnalyticsMigration;

[ExcludeFromCodeCoverage]
public class VacancyAnalyticsMigrationTimerTrigger(
    ILogger<VacancyAnalyticsMigrationTimerTrigger> logger,
    VacancyAnalyticsMigrationStrategy vacancyAnalyticsMigrationStrategy)
{
    private const string TriggerName = nameof(VacancyAnalyticsMigrationTimerTrigger);

    // Function attribute is commented out to prevent accidental execution of the migration. To enable the migration, uncomment the Function attribute and ensure the timer trigger schedule is appropriate for your needs.
    //[Function(TriggerName)]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timerInfo, CancellationToken cancellationToken)
    {
        return;

        // This trigger is intended to run once a day,
        // migrating vacancy analytics data from MongoDB to SQL.
        // The timer trigger should be the primary trigger for the migration, with the HTTP trigger available as a backup if required.
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await vacancyAnalyticsMigrationStrategy.RunAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during migration", TriggerName);
            throw;
        }
        finally
        {
            logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
        }
    }
}