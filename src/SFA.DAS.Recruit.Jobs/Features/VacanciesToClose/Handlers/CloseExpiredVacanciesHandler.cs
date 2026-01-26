using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Recruit.Jobs.Domain.Events;
using SFA.DAS.Recruit.Jobs.OuterApi;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToClose.Handlers;

public interface ICloseExpiredVacanciesHandler
{
    Task RunAsync(CancellationToken cancellationToken);
}
public class CloseExpiredVacanciesHandler(ILogger<CloseExpiredVacanciesHandler> logger,
    IMessageSession messaging,
    IRecruitJobsOuterClient jobsOuterClient) : ICloseExpiredVacanciesHandler
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting deletion of delayed notifications for inactive users.");

        try
        {
            var pointInTime = DateTime.UtcNow;

            // Fetch vacancies that are expired as of pointInTime
            var response = await jobsOuterClient.GetVacanciesToCloseAsync(pointInTime, cancellationToken);

            if (!response.Success || response.Payload is null || !response.Payload.Data.Any())
            {
                logger.LogInformation("No expired vacancies found for pointInTime: {DateTime}.", pointInTime);
                return;
            }

            // Process and delete notifications in a safe loop
            foreach (var vacancy in response.Payload!.Data)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await messaging.Publish(new VacancyClosedEvent
                {
                    VacancyId = vacancy.Id,
                    VacancyReference = vacancy.VacancyReference
                });

                logger.LogInformation("Successfully closed vacancy with Id : {Id}", vacancy.Id);
            }

            logger.LogInformation("Marked all expired vacancies to close.");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("CloseExpiredVacanciesHandler was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred while closing expired vacancies.");
            throw;
        }
    }
}