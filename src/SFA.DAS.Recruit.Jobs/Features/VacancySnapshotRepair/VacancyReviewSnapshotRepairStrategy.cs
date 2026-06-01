using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SFA.DAS.Recruit.Jobs.Features.VacancySnapshotRepair;

[ExcludeFromCodeCoverage]
public class VacancyReviewSnapshotRepairStrategy(ILogger<VacancyReviewSnapshotRepairStrategy> logger,
    VacancyReviewSnapshotRepairSqlRepository sqlRepository)
{
    private const int BatchSize = 100;
    private const int MaxRuntimeInSeconds = 270; // 4m 30s
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            var vacancyReviews = await sqlRepository.GetVacancyReviewSnapshotsBatchAsync(BatchSize, cancellationToken);
            if (vacancyReviews.Count == 0)
                break;
            foreach (var vacancyReview in vacancyReviews)
            {
                var vacancy = await sqlRepository.GetVacancyByVacancyReferenceAsync(vacancyReview.VacancyReference, cancellationToken);
                if (vacancy != null)
                {
                    if (vacancy.AccountId != null) vacancyReview.AccountId = Convert.ToInt64(vacancy.AccountId);
                    if (vacancy.AccountLegalEntityId != null) vacancyReview.AccountLegalEntityId = Convert.ToInt64(vacancy.AccountLegalEntityId);
                    if (vacancy.Ukprn != null) vacancyReview.Ukprn = Convert.ToInt64(vacancy.Ukprn);
                    if (vacancy.OwnerType != null) vacancyReview.OwnerType = (OwnerType)vacancy.OwnerType;
                    vacancyReview.VacancySnapshot = JsonConvert.SerializeObject(vacancy);
                }
                else
                {
                    logger.LogInformation("Vacancy not found for VacancyReference: {VacancyReference}", vacancyReview.VacancyReference);
                }
            }
            await ProcessBatchAsync(vacancyReviews);
        }
    }
    private async Task ProcessBatchAsync(List<VacancyReview> batch)
    {
        await sqlRepository.UpsertVacancyReviewSnapshotsBatchAsync(batch);
    }

    private static string ToVacancySnapshot(Vacancy vacancy)
    {
        return JsonSerializer.Serialize(vacancy, options: new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });
    }
}