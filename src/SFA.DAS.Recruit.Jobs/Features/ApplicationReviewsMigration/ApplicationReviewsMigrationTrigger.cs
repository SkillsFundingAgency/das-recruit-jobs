using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

public class ApplicationReviewsMigrationTrigger(
    ILogger<ApplicationReviewsMigrationTrigger> logger,
    ApplicationReviewMigrationStrategy applicationReviewMigrationStrategy)
{
    private const string TriggerName = nameof(ApplicationReviewsMigrationTrigger);
    
    public async Task MigrationApplicationReviewsAsync([TimerTrigger("0 0 * * *", RunOnStartup = true)] TimerInfo _)
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