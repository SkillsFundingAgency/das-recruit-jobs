using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.UserMigration;

[ExcludeFromCodeCoverage]
public class UserMigrationTimerTrigger(ILogger<UserMigrationTimerTrigger> logger, UserMigrationStrategy userMigrationStrategy)
{
    private const string TriggerName = nameof(UserMigrationTimerTrigger);
    
    //[Function(TriggerName)]
    public async Task Run([TimerTrigger("*/5 23-3 * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await userMigrationStrategy.RunAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during migration", TriggerName);
            throw;
        }
        logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
    }
}