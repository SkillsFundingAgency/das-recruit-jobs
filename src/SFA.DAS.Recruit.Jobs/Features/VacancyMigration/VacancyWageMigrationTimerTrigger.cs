using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

[ExcludeFromCodeCoverage]
public class VacancyWageMigrationTimerTrigger(
    ILogger<VacancyWageMigrationTimerTrigger> logger,
    VacancyWageMigrationStrategy vacancyWageMigrationStrategy)
{
    private const string TriggerName = nameof(VacancyWageMigrationTimerTrigger);
    
    [Function(TriggerName)]
    public async Task Run([TimerTrigger("*/5 23-3 * * *", RunOnStartup = true)] TimerInfo timerInfo)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await vacancyWageMigrationStrategy.RunAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during migration", TriggerName);
            throw;
        }
        logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
    }
}