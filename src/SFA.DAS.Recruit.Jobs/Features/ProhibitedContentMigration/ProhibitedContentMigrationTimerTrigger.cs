using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.ProhibitedContentMigration;

[ExcludeFromCodeCoverage]
public class ProhibitedContentMigrationTimerTrigger(
    ILogger<ProhibitedContentMigrationTimerTrigger> logger,
    ProhibitedContentMigrationStrategy prohibitedContentMigrationStrategy)
{
    private const string TriggerName = nameof(ProhibitedContentMigrationTimerTrigger);
    
    [Function(TriggerName)]
    public async Task Run([TimerTrigger("0 5 * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await prohibitedContentMigrationStrategy.RunAsync();
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