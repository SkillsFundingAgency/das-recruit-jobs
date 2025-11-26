using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

[ExcludeFromCodeCoverage]
public class VacancyMigrationTimerTrigger(ILogger<VacancyMigrationTimerTrigger> logger, VacancyMigrationStrategy vacancyMigrationStrategy)
{
    private const string TriggerName = nameof(VacancyMigrationTimerTrigger);
    
    [Function(TriggerName)] // disable until the Users have properly migrated
    public async Task Run([TimerTrigger("*/15 * * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await vacancyMigrationStrategy.RunAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during migration", TriggerName);
            throw;
        }
        logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
    }
}