using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;

[ExcludeFromCodeCoverage]
public class UserNotificationPreferencesMigrationHttpTrigger(
    ILogger<UserNotificationPreferencesMigrationHttpTrigger> logger,
    UserNotificationPreferencesMigrationStrategy userNotificationPreferencesMigrationStrategy)
{
    private const string TriggerName = nameof(UserNotificationPreferencesMigrationHttpTrigger);
    
    [Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post",Route = null)] HttpRequestData requestData)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            var request = await JsonSerializer.DeserializeAsync<MigrateUserNotificationPreferencesHttpRequest>(requestData.Body);
            if (request?.Ids is { Count: > 0 })
            {
                await userNotificationPreferencesMigrationStrategy.RunAsync(request.Ids);
            }
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