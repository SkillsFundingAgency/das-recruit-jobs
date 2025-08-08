using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyReviewMigration;

[ExcludeFromCodeCoverage]
public class VacancyReviewMigrationTimerTrigger(
    ILogger<VacancyReviewMigrationTimerTrigger> logger,
    VacancyReviewMigrationStrategy vacancyReviewMigrationStrategy)
{
    private const string TriggerName = nameof(VacancyReviewMigrationTimerTrigger);
    
    //[Function(TriggerName)] // disable as migration is complete
    public async Task Run([TimerTrigger("0-10/5 5 * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            //await vacancyReviewMigrationStrategy.RunAsync();
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