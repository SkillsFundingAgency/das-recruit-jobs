using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToClose.Handlers;

public interface ICloseExpiredVacanciesHandler
{
    Task RunAsync(FunctionContext context, CancellationToken cancellationToken);
}
public class CloseExpiredVacanciesHandler(ILogger<CloseExpiredVacanciesHandler> logger,
    IFunctionEndpoint endpoint,
    IRecruitJobsOuterClient jobsOuterClient) : ICloseExpiredVacanciesHandler
{
    public async Task RunAsync(FunctionContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Initialising the process of closing expired vacancies.");

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

                var closureResponse = await jobsOuterClient.PostVacancyToClose(vacancy.Id, vacancy.VacancyReference, ClosureReason.Auto, cancellationToken);
                if (closureResponse.Success)
                {
                    var command = new VacancyClosedEvent
                    {
                        VacancyId = vacancy.Id,
                        VacancyReference = vacancy.VacancyReference
                    };
                    var options = new SendOptions();
                    options.SetDestination(StorageConstants.QueueNames.FindApprenticeshipJobsQueue);
                    await endpoint.Send(command, options, context, cancellationToken);
                    logger.LogInformation("Successfully closed vacancy with Id : {Id}", vacancy.Id);
                }
                else
                {
                    logger.LogInformation("unable to mark the vacancy: {vacancyId} to close. Reason: {reason}", vacancy.Id, closureResponse.ErrorContent);
                }
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