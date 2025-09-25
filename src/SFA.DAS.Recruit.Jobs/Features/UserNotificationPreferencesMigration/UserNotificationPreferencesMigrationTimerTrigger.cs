using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;

[ExcludeFromCodeCoverage]
public class UserNotificationPreferencesMigrationTimerTrigger(
    ILogger<UserNotificationPreferencesMigrationTimerTrigger> logger,
    UserNotificationPreferencesMigrationStrategy userNotificationPreferencesMigrationStrategy)
{
    private const string TriggerName = nameof(UserNotificationPreferencesMigrationTimerTrigger);
    
    //[Function(TriggerName)] // disable until the Users have properly migrated
    public async Task Run([TimerTrigger("0/5 * * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await userNotificationPreferencesMigrationStrategy.RunAsync();
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