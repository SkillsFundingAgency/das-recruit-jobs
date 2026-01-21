using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi;

namespace SFA.DAS.Recruit.Jobs.Features.DeleteStaleVacancies.Handlers;

public interface IDeleteStaleVacanciesHandler
{
    Task RunAsync(CancellationToken cancellationToken);
}
public class DeleteStaleVacanciesHandler(ILogger<DeleteStaleVacanciesHandler> logger,
    IRecruitJobsOuterClient jobsOuterClient) : IDeleteStaleVacanciesHandler
{
    private const int DefaultDraftStaleByDays = 180; // 6 months
    private const int DefaultReferredStaleByDays = 90; // 3 months
    private const int DefaultRejectedStaleByDays = 90; // 3 months

    private const int DeleteBatchParallelism = 10; // tune based on API limits

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting deletion of stale vacancies.");

        var pointInTime = DateTime.UtcNow;
        var draftStaleByDate = pointInTime.AddDays(-DefaultDraftStaleByDays);
        var referredStaleByDate = pointInTime.AddDays(-DefaultReferredStaleByDays);
        var rejectedStaleByDate = pointInTime.AddDays(-DefaultRejectedStaleByDays);

        // Fetch in parallel
        var fetchTasks = await Task.WhenAll(
            jobsOuterClient.GetDraftVacanciesToCloseAsync(draftStaleByDate, cancellationToken),
            jobsOuterClient.GetEmployerReviewedVacanciesToClose(referredStaleByDate, cancellationToken),
            jobsOuterClient.GetRejectedEmployerVacanciesToClose(rejectedStaleByDate, cancellationToken)
        );

        var totalStaleVacancies = new HashSet<Guid>();

        AddStaleVacancies(fetchTasks[0], VacancyStatus.Draft, draftStaleByDate, totalStaleVacancies);
        AddStaleVacancies(fetchTasks[1], VacancyStatus.Referred, referredStaleByDate, totalStaleVacancies);
        AddStaleVacancies(fetchTasks[2], VacancyStatus.Rejected, rejectedStaleByDate, totalStaleVacancies);

        if (totalStaleVacancies.Count == 0)
        {
            logger.LogInformation("No stale vacancies found to delete.");
            return;
        }

        logger.LogInformation("Deleting {Count} stale vacancies.", totalStaleVacancies.Count);

        // Parallel delete with controlled concurrency
        await Parallel.ForEachAsync(
            totalStaleVacancies,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = DeleteBatchParallelism
            },
            async (vacancyId, token) =>
            {
                await jobsOuterClient.DeleteVacancyAsync(vacancyId, token);
            });

        logger.LogInformation("Finished deleting vacancies that have not been updated since {Date}", pointInTime.ToShortDateString());
    }

    // Helper method to add vacancies to the target set
    private void AddStaleVacancies(ApiResponse<OuterApi.Common.StaleVacancies>? response,
        VacancyStatus status,
        DateTime staleByDate,
        HashSet<Guid> target)
    {
        if (response is not { Success: true, Payload.Data: not null } || !response.Payload.Data.Any())
        {
            return;
        }

        // Filter and add IDs to the target set
        var ids = response.Payload.Data
            .Where(v => v.Status == status)
            .Select(v => v.Id)
            .ToList();

        foreach (var id in ids)
        {
            target.Add(id);
        }

        logger.LogInformation(
            "Found {Count} {Status} vacancies created on or before {StaleDate}",
            ids.Count,
            status,
            staleByDate);
    }
}