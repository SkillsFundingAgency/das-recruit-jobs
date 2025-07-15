using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.EmployerProfilesMigration;

[ExcludeFromCodeCoverage]
public class EmployerProfilesMigrationTimerTrigger(
    ILogger<EmployerProfilesMigrationTimerTrigger> logger,
    EmployerProfilesMigrationStrategy employerProfilesMigrationStrategy)
{
    private const string TriggerName = nameof(EmployerProfilesMigrationTimerTrigger);
    
    [Function(TriggerName)]
    public async Task Run([TimerTrigger("15-25/5 4 * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await employerProfilesMigrationStrategy.RunAsync();
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