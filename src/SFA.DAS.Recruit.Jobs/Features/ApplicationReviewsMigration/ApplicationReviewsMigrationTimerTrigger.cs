using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

public class ApplicationReviewsMigrationTimerTrigger(
    ILogger<ApplicationReviewsMigrationTimerTrigger> logger,
    ApplicationReviewMigrationStrategy applicationReviewMigrationStrategy)
{
    private const string TriggerName = nameof(ApplicationReviewsMigrationTimerTrigger);
    
    public async Task MigrationApplicationReviewsAsync([TimerTrigger("0 0 * * *")] TimerInfo _)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await applicationReviewMigrationStrategy.RunAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during migration", TriggerName);
            throw;
        }
        logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
    }
}