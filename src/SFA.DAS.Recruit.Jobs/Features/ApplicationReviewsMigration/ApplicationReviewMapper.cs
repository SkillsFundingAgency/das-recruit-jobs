using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using SqlApplicationReviewStatus = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.ApplicationReviewStatus;
using MongoApplicationReviewStatus = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.ApplicationReviewStatus;
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
        if (userIdParsedOk is false && source is { StatusUpdatedBy.UserId : not null })
        {
            logger.LogWarning("[{ApplicationReviewId}] Failed to parse StatusUpdatedBy from value: '{sourceValue}'", source.Id, source.StatusUpdatedBy?.UserId);
        }

        if (!Enum.TryParse(vacancy?.OwnerType, true, out Owner owner))
        {
            logger.LogWarning("[{ApplicationReviewId}] Failed to parse OwnerType from value: '{sourceValue}'", source.Id, vacancy?.OwnerType);
        }

        // currently TryDecode throws if null/"" is passed :/
        long accountId = 0;
        if (vacancy?.EmployerAccountId is null || !encodingService.TryDecode(vacancy?.EmployerAccountId, EncodingType.AccountId, out accountId))
        {
            logger.LogWarning("[{ApplicationReviewId}] Failed to decode EmployerAccountId from value: '{sourceValue}'", source.Id, vacancy?.EmployerAccountId);
        }

        long accountLegalEntityId = 0;
        if (vacancy?.AccountLegalEntityPublicHashedId is null || !encodingService.TryDecode(vacancy?.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId, out accountLegalEntityId))
        {
            logger.LogWarning("[{ApplicationReviewId}] Failed to decode AccountLegalEntityPublicHashedId from value: '{sourceValue}'", source.Id, vacancy?.AccountLegalEntityPublicHashedId);
        }

        var vacancyTitle = vacancy?.Title;
        if (string.IsNullOrWhiteSpace(vacancyTitle))
        {
	        logger.LogWarning("[{ApplicationReviewId}] Referenced Vacancy has no title: '{VacancyReference}'", source.Id, source.VacancyReference);
        }
        
        return new SqlApplicationReview
        {
            AccountId = accountId,
            AccountLegalEntityId = accountLegalEntityId,
            AdditionalQuestion1 = source.Application?.AdditionalQuestion1,
            AdditionalQuestion2 = source.Application?.AdditionalQuestion2,
            ApplicationId = source.Application?.IsFaaV2Application is true ? source.Application?.ApplicationId : null,
            CandidateFeedback = source.CandidateFeedback,
            CandidateId = source.CandidateId,
            CreatedDate = source.CreatedDate,
            DateSharedWithEmployer = source.DateSharedWithEmployer,
            EmployerFeedback = source.EmployerFeedback,
            HasEverBeenEmployerInterviewing = source.HasEverBeenEmployerInterviewing ?? false,
            Id = source.Id,
            LegacyApplicationId = source.Application?.IsFaaV2Application is false ? null : source.Id,
            Owner = owner,
            ReviewedDate = source.ReviewedDate,
            Status = MapStatus(source.Status),
            StatusUpdatedBy = null, // TODO: how do we determine this?
            StatusUpdatedByUserId = userIdParsedOk ? statusUpdatedByUserId : null,
            SubmittedDate = source.SubmittedDate,
            Ukprn = (int)(vacancy?.TrainingProvider?.Ukprn ?? -1),
            VacancyReference = source.VacancyReference,
            VacancyTitle = vacancyTitle ?? string.Empty,
            WithdrawnDate = source.WithdrawnDate,
        };
    }

    private static SqlApplicationReviewStatus MapStatus(DataAccess.MongoDb.Domain.ApplicationReviewStatus source)
    {
        return source switch
        {
            MongoApplicationReviewStatus.New => SqlApplicationReviewStatus.New,
            MongoApplicationReviewStatus.Successful => SqlApplicationReviewStatus.Successful,
            MongoApplicationReviewStatus.Unsuccessful => SqlApplicationReviewStatus.Unsuccessful,
            MongoApplicationReviewStatus.Shared => SqlApplicationReviewStatus.Shared,
            MongoApplicationReviewStatus.InReview => SqlApplicationReviewStatus.InReview,
            MongoApplicationReviewStatus.Interviewing => SqlApplicationReviewStatus.Interviewing,
            MongoApplicationReviewStatus.EmployerInterviewing => SqlApplicationReviewStatus.EmployerInterviewing,
            MongoApplicationReviewStatus.EmployerUnsuccessful => SqlApplicationReviewStatus.EmployerUnsuccessful,
            MongoApplicationReviewStatus.PendingShared => SqlApplicationReviewStatus.PendingShared,
            MongoApplicationReviewStatus.PendingToMakeUnsuccessful => SqlApplicationReviewStatus.PendingToMakeUnsuccessful,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}