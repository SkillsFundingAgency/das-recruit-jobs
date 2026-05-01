using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using System.Diagnostics.CodeAnalysis;
using SqlVacancy = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.Vacancy;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive;

[ExcludeFromCodeCoverage]
public class VacancyArchivingStrategy(VacancyArchivingSqlRepository sqlRepository)
{
    private const int BatchSize = 200;
    private const int MaxRuntimeInSeconds = 270; // 4m 30s
    private const int DefaultArchiveStaleByDays = 182; // 6 months and 2 days to account for leap year

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var pointInTime = startTime;
            var archiveStaleByDate = pointInTime.AddDays(-DefaultArchiveStaleByDays);

            while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
            {
                var vacancies = await sqlRepository.GetClosedVacancies(
                    archiveStaleByDate,
                    BatchSize,
                    cancellationToken);

                if (vacancies.Count == 0)
                    break;

                await ProcessBatchAsync(vacancies);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task ProcessBatchAsync(List<SqlVacancy> batch)
    {
        var now = DateTime.UtcNow;

        foreach (var vacancy in batch)
        {
            vacancy.ArchivedDate = now;
            vacancy.ArchiveType = ArchiveType.Auto;
            vacancy.Status = VacancyStatus.Archived;
        }

        await sqlRepository.UpsertVacanciesBatchAsync(batch);
    }
}