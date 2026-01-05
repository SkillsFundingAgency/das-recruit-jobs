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
    
    [Function(TriggerName)]
    public async Task Run([TimerTrigger("0 0 0 * * ?")] TimerInfo timerInfo, CancellationToken cancellationToken)
    {
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