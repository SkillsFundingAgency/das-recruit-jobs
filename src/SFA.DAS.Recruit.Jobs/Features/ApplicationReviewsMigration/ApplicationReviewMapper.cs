using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using ApplicationReviewStatus = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.ApplicationReviewStatus;
using SqlApplicationReview = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.ApplicationReview;
using MongoApplicationReview = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.ApplicationReview;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

public class ApplicationReviewMapper(ILogger<ApplicationReviewMapper> logger, IEncodingService encodingService)
{
    public SqlApplicationReview MapFrom(MongoApplicationReview source, List<Vacancy> vacancies)
    {
        var vacancy = vacancies.FirstOrDefault(x => x.VacancyReference == source.VacancyReference);
        if (vacancy is null)
        {
            logger.LogWarning("[{ApplicationReviewId}] Failed to find associated vacancy: '{vacancyReference}'", source.Id, source.VacancyReference);
        }
        
        var userIdParsedOk = Guid.TryParse(source.StatusUpdatedBy?.UserId, out var statusUpdatedByUserId);
        if (userIdParsedOk is false && (source.StatusUpdatedBy?.UserId.Length ?? 0) > 0)
        {
            logger.LogWarning("[{ApplicationReviewId}] Failed to parse StatusUpdatedBy from value: '{sourceValue}'", source.Id, source.StatusUpdatedBy?.UserId);
        }

        if (!Enum.TryParse(vacancy?.OwnerType, true, out Owner owner))
        {
            logger.LogWarning("[{ApplicationReviewId}] Failed to parse OwnerType from value: '{sourceValue}'", source.Id, vacancy?.OwnerType);
        }

        var accountId = encodingService.Decode(vacancy?.EmployerAccountId, EncodingType.AccountId);
        return new SqlApplicationReview
        {
            AccountId = accountId,
            ApplicationId = source.Application.IsFaaV2Application ? source.Application.ApplicationId : null,
            CandidateFeedback = source.CandidateFeedback,
            CandidateId = source.CandidateId,
            CreatedDate = source.CreatedDate,
            DateSharedWithEmployer = source.DateSharedWithEmployer,
            HasEverBeenEmployerInterviewing = source.HasEverBeenEmployerInterviewing ?? false,
            Id = source.Id,
            LegacyApplicationId = source.Application.IsFaaV2Application ? null : null, // TODO: needs importing the data
            Owner = owner,
            ReviewedDate = source.ReviewedDate,
            Status = MapStatus(source.Status),
            StatusUpdatedBy = null, // TODO: needs looking up
            StatusUpdatedByUserId = userIdParsedOk ? statusUpdatedByUserId : null,
            SubmittedDate = source.SubmittedDate,
            Ukprn = (int)(vacancy?.TrainingProvider?.Ukprn ?? -1),
            VacancyReference = source.VacancyReference,
            WithdrawnDate = source.WithdrawnDate,
        };
        
    }

    private static ApplicationReviewStatus MapStatus(DataAccess.MongoDb.Domain.ApplicationReviewStatus source)
    {
        return source switch
        {
            DataAccess.MongoDb.Domain.ApplicationReviewStatus.New => ApplicationReviewStatus.New,
            DataAccess.MongoDb.Domain.ApplicationReviewStatus.Successful => ApplicationReviewStatus.Successful,
            DataAccess.MongoDb.Domain.ApplicationReviewStatus.Unsuccessful => ApplicationReviewStatus.Unsuccessful,
            DataAccess.MongoDb.Domain.ApplicationReviewStatus.Shared => ApplicationReviewStatus.Shared,
            DataAccess.MongoDb.Domain.ApplicationReviewStatus.InReview => ApplicationReviewStatus.InReview,
            DataAccess.MongoDb.Domain.ApplicationReviewStatus.Interviewing => ApplicationReviewStatus.Interviewing,
            DataAccess.MongoDb.Domain.ApplicationReviewStatus.EmployerInterviewing => ApplicationReviewStatus.EmployerInterviewing,
            DataAccess.MongoDb.Domain.ApplicationReviewStatus.EmployerUnsuccessful => ApplicationReviewStatus.EmployerUnsuccessful,
            DataAccess.MongoDb.Domain.ApplicationReviewStatus.PendingShared => ApplicationReviewStatus.PendingShared,
            DataAccess.MongoDb.Domain.ApplicationReviewStatus.PendingToMakeUnsuccessful => ApplicationReviewStatus.PendingToMakeUnsuccessful,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}