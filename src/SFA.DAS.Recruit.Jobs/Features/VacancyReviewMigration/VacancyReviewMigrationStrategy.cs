using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using SqlVacancyReview = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.VacancyReview;
using MongoVacancyReview = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.VacancyReview;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyReviewMigration;

[ExcludeFromCodeCoverage]
public class VacancyReviewMigrationStrategy(
    VacancyReviewMapper mapper,
    VacancyReviewMigrationMongoRepository mongoRepository,
    VacancyReviewMigrationSqlRepository sqlRepository)
{
    private const int BatchSize = 100;
    private const int MaxAgeInDays = 120; // ~4 months
    private const int MaxRuntimeInSeconds = 270; // 4m 30s
    
    public async Task RunAsync()
    {
        var startTime = DateTime.UtcNow;
        var mongoVacancyReviews = await mongoRepository.FetchBatchAsync(BatchSize, MaxAgeInDays);
        while (mongoVacancyReviews is { Count: > 0 } && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(MaxRuntimeInSeconds))
        {
            await ProcessBatchAsync(mongoVacancyReviews);
            mongoVacancyReviews = await mongoRepository.FetchBatchAsync(BatchSize, MaxAgeInDays);
        }
    }

    private async Task ProcessBatchAsync(List<MongoVacancyReview> mongoVacancyReviews)
    {
        var apprenticeships = mongoVacancyReviews.Where(x => x.VacancySnapshot.VacancyType is null or VacancyType.Apprenticeship).ToList();
        var traineeships = mongoVacancyReviews.Except(apprenticeships).ToList();

        if (apprenticeships is { Count: > 0 })
        {
            List<MongoVacancyReview> excluded = [];
            var mappedVacancyReviews = apprenticeships
                .Select(x => {
                    var item = mapper.MapFrom(x);
                    if (item == SqlVacancyReview.None)
                    {
                        excluded.Add(x);
                    }

                    return item;
                })
                .Where(x => x != SqlVacancyReview.None)
                .ToList();

            if (excluded is { Count: > 0 })
            {
                await mongoRepository.UpdateFailedMigrationDateBatchAsync(excluded.Select(x => x.Id).ToList());
            }

            if (mappedVacancyReviews is { Count: > 0 })
            {
                await sqlRepository.UpsertVacancyReviewBatchAsync(mappedVacancyReviews);
                await mongoRepository.UpdateSuccessMigrationDateBatchAsync(mappedVacancyReviews.Select(x => x.Id).ToList());
            }
        }

        if (traineeships is { Count: > 0 })
        {
            await mongoRepository.UpdateIgnoreMigrationBatchAsync(traineeships.Select(x => x.Id).ToList());
        }
    }
}