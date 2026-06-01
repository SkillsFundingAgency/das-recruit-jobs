using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Vacancy;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Address = SFA.DAS.Recruit.Jobs.Domain.Address;
using JsonSerializer = System.Text.Json.JsonSerializer;
using OwnerType = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.OwnerType;
using VacancyReview = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.VacancyReview;

namespace SFA.DAS.Recruit.Jobs.Features.VacancySnapshotRepair;

[ExcludeFromCodeCoverage]
public class VacancyReviewSnapshotRepairStrategy(ILogger<VacancyReviewSnapshotRepairStrategy> logger,
    IEncodingService encodingService,
    IRecruitJobsOuterClient recruitJobsOuterClient,
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
                //var vacancy = await sqlRepository.GetVacancyByVacancyReferenceAsync(vacancyReview.VacancyReference, cancellationToken);
                var vacancy = await recruitJobsOuterClient.GetVacancyAsync(vacancyReview.VacancyReference, cancellationToken);
                if (vacancy.Payload != null)
                {
                    var vacancyResponse = vacancy.Payload.Data;
                    if (vacancyResponse.AccountId != null) vacancyReview.AccountId = Convert.ToInt64(vacancyResponse.AccountId);
                    if (vacancyResponse.AccountLegalEntityId != null) vacancyReview.AccountLegalEntityId = Convert.ToInt64(vacancyResponse.AccountLegalEntityId);
                    if (vacancyResponse.TrainingProvider?.Ukprn != null) vacancyReview.Ukprn = Convert.ToInt64(vacancyResponse.TrainingProvider.Ukprn);
                    if (vacancyResponse.OwnerType != null) vacancyReview.OwnerType = (OwnerType)vacancyResponse.OwnerType;
                    vacancyReview.VacancySnapshot = ToVacancySnapshot(vacancyResponse);
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

    private string ToVacancySnapshot(VacancyResponse vacancy)
    {
        vacancy.EmployerAccountId = encodingService.Encode(vacancy.AccountId ?? 0, EncodingType.AccountId);
        vacancy.AccountLegalEntityPublicHashedId = encodingService.Encode(vacancy.AccountLegalEntityId ?? 0, EncodingType.PublicAccountLegalEntityId);

        if (vacancy.SubmittedByUserId is not null)
        {
            vacancy.SubmittedByUser = new VacancyUser
            {
                UserId = vacancy.SubmittedByUserId.ToString()
            };
        }
        if (vacancy.EmployerLocation is not null)
        {
            vacancy.EmployerLocation = new Address
            {
                AddressLine1 = vacancy.EmployerLocation?.AddressLine1,
                AddressLine2 = vacancy.EmployerLocation?.AddressLine2,
                AddressLine3 = vacancy.EmployerLocation?.AddressLine3,
                Postcode = vacancy.EmployerLocation?.Postcode,
                Latitude = vacancy.EmployerLocation?.Latitude,
                Longitude = vacancy.EmployerLocation?.Longitude
            };
        }
        if (vacancy.EmployerContact is not null)
        {
            vacancy.EmployerContact = new ContactDetail
            {
                Name = vacancy.EmployerContact?.Name,
                Email = vacancy.EmployerContact?.Email,
                Phone = vacancy.EmployerContact?.Phone
            };
        }
        if (vacancy.ProviderContact is not null)
        {
            vacancy.ProviderContact = new ContactDetail
            {
                Name = vacancy.ProviderContact?.Name,
                Email = vacancy.ProviderContact?.Email,
                Phone = vacancy.ProviderContact?.Phone
            };
        }

        return JsonSerializer.Serialize(vacancy, options: new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });
    }
}