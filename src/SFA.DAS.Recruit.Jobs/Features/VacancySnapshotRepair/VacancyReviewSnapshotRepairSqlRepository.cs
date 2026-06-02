using Microsoft.EntityFrameworkCore;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.VacancySnapshotRepair;

public class VacancyReviewSnapshotRepairSqlRepository(RecruitJobsDataContext dataContext)
{
    public async Task UpsertVacancyReviewSnapshotsBatchAsync(List<VacancyReview> vacancyReviews)
    {
        foreach (var vacancyReview in vacancyReviews)
        {
            var existingSnapshot = await dataContext.VacancyReview.FirstOrDefaultAsync(x => x.Id == vacancyReview.Id);

            if (existingSnapshot != null)
            {
                // Only update the snapshot and account details if the existing snapshot has not been updated before (i.e. account details are 0 and snapshot is null or empty)
                await dataContext.VacancyReview
                    .Where(x => x.Id == vacancyReview.Id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.AccountId, vacancyReview.AccountId)
                        .SetProperty(x => x.AccountLegalEntityId, vacancyReview.AccountLegalEntityId)
                        .SetProperty(x => x.OwnerType, vacancyReview.OwnerType)
                        .SetProperty(x => x.Ukprn, vacancyReview.Ukprn)
                        .SetProperty(x => x.VacancySnapshot, vacancyReview.VacancySnapshot)
                    );
            }
        }

        await dataContext.SaveChangesAsync();
    }

    public async Task<List<VacancyReview>> GetVacancyReviewSnapshotsBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        return await dataContext
            .VacancyReview
            .Where(x => x.AccountId == 0 && x.AccountLegalEntityId == 0 && x.Ukprn == 0)
            .Take(batchSize)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<Vacancy?> GetVacancyByVacancyReferenceAsync(long vacancyReference, CancellationToken cancellationToken)
    {
        return await dataContext
            .Vacancy
            .FirstOrDefaultAsync(x => x.VacancyReference == vacancyReference, cancellationToken: cancellationToken);
    }
}
