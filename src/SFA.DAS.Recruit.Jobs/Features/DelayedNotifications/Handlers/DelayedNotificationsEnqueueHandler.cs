using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Clients;
using SFA.DAS.Recruit.Jobs.OuterApi;

namespace SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Handlers;

public interface IDelayedNotificationsEnqueueHandler
{
    Task RunAsync(CancellationToken cancellationToken);
}

public class DelayedNotificationsEnqueueHandler(
    ILogger<DelayedNotificationsEnqueueHandler> logger, 
    IDelayedNotificationQueueClient queueClient,
    IRecruitJobsOuterClient jobsOuterClient): IDelayedNotificationsEnqueueHandler
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var canContinue = true;
        while (!cancellationToken.IsCancellationRequested && canContinue)
        {
            canContinue = await EnqueueEmailsAsync(cancellationToken);
        }
    }
    
    private async Task<bool> EnqueueEmailsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await jobsOuterClient.GetDelayedNotificationsBatchBeforeDateAsync(DateTime.UtcNow, cancellationToken);
            if (!response.Success || response.Payload is not { Count: > 0 })
            {
                return false;
            }
        
            foreach (var email in response.Payload)
            {
                var deleteResponse = await jobsOuterClient.DeleteDelayedNotificationsAsync(email.SourceIds);
                if (!deleteResponse.Success)
                {
                    logger.LogInformation("Request to delete emails failed with status code '{StatusCode}' and error content '{ErrorContent}'", deleteResponse.StatusCode, deleteResponse.ErrorContent);
                    return false;
                }

                await queueClient.SendMessageAsync(email);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        
        return true;
    }
}