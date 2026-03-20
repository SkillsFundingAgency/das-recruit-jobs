using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.OuterApi;

namespace SFA.DAS.Recruit.Jobs.Features.DelayedNotifications.Handlers;

public interface IDeleteNotificationsForInactiveUsersHandler
{
    Task RunAsync(CancellationToken cancellationToken);
}

public class DeleteNotificationsForInactiveUsersHandler(
    ILogger<DeleteNotificationsForInactiveUsersHandler> logger,
    IRecruitJobsOuterClient jobsOuterClient) : IDeleteNotificationsForInactiveUsersHandler
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting deletion of delayed notifications for inactive users.");

        try
        {
            // Fetch delayed notifications for inactive users
            var response = await jobsOuterClient.GetDelayedNotificationsBatchByUsersInactiveStatus(cancellationToken);

            if (!response.Success || response.Payload is not { Count: > 0 })
            {
                logger.LogInformation("No delayed notifications found for inactive users.");
                return;
            }

            // Process and delete notifications in a safe loop
            foreach (var email in response.Payload)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!email.SourceIds.Any())
                {
                    logger.LogWarning("Skipping user as SourceIds list is empty.");
                    continue;
                }

                var deleteResponse = await jobsOuterClient.DeleteDelayedNotificationsAsync(email.SourceIds);

                if (!deleteResponse.Success)
                {
                    logger.LogError(
                        "Failed to delete delayed notifications for SourceIds [{SourceIds}]. " +
                        "StatusCode: {StatusCode}, Error: {ErrorContent}",
                        string.Join(",", email.SourceIds),
                        deleteResponse.StatusCode,
                        deleteResponse.ErrorContent);

                    continue;
                }

                logger.LogInformation(
                    "Successfully deleted delayed notifications for user with SourceIds [{SourceIds}].",
                    string.Join(",", email.SourceIds));
            }

            logger.LogInformation("Completed deletion of delayed notifications for inactive users.");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("DeleteNotificationsForInactiveUsersHandler was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred while deleting delayed notifications for inactive users.");
            throw;
        }
    }
}