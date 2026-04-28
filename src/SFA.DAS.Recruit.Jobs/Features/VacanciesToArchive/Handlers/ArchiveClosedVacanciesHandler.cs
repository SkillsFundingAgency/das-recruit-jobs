using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Handlers;

public interface IArchiveClosedVacanciesHandler
{
    Task RunAsync(CancellationToken cancellationToken);
}

public class ArchiveClosedVacanciesHandler(ILogger<ArchiveClosedVacanciesHandler> logger,
    IOptions<Core.Configuration.Features> features,
    IRecruitJobsOuterClient jobsOuterClient) : IArchiveClosedVacanciesHandler
{
    private const int DefaultArchiveStaleByDays = 182; // 6 months and 2 days to account for leap year
    private readonly Core.Configuration.Features _features = features.Value;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Initialising the process of archiving expired vacancies.");

        try
        {
            var pointInTime = DateTime.UtcNow;
            var archiveStaleByDate = pointInTime.AddDays(-DefaultArchiveStaleByDays);
            var includeVacanciesWithOutCome = _features.ArchiveVacanciesWithoutOutCome;

            // Fetch vacancies that are expired as of pointInTime
            var response = await jobsOuterClient.GetVacanciesToArchiveAsync(archiveStaleByDate, includeVacanciesWithOutCome, cancellationToken);

            if (!response.Success || response.Payload is null || !response.Payload.Data.Any())
            {
                logger.LogInformation("No closed vacancies found for pointInTime: {DateTime}.", archiveStaleByDate);
                return;
            }

            // Process and delete notifications in a safe loop
            foreach (var vacancy in response.Payload!.Data.Where(x => 
                         x.Status == VacancyStatus.Closed && 
                         x.ClosingDate <= archiveStaleByDate))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var archiveResponse = await jobsOuterClient.PostVacancyToArchive(vacancy.Id, vacancy.VacancyReference, cancellationToken);
                if (archiveResponse.Success)
                {
                    logger.LogInformation("Successfully archived vacancy with Id : {Id}", vacancy.Id);
                }
                else
                {
                    logger.LogInformation("Unable to archive the vacancy: {vacancyId}. Reason: {reason}", vacancy.Id, archiveResponse.ErrorContent);
                }
            }

            logger.LogInformation("Marked all closed vacancies to archive.");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("ArchiveClosedVacanciesHandler was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred while archiving vacancies.");
            throw;
        }
    }
}
