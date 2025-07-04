﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using SqlApplicationReviewStatus = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.ApplicationReviewStatus;
using MongoApplicationReviewStatus = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.ApplicationReviewStatus;
using SqlApplicationReview = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.ApplicationReview;
using MongoApplicationReview = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.ApplicationReview;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

[ExcludeFromCodeCoverage]
public class ApplicationReviewMapper(ILogger<ApplicationReviewMapper> logger, IEncodingService encodingService)
{
    public SqlApplicationReview MapFrom(MongoApplicationReview source, List<Vacancy> vacancies)
    {
        // We should be guaranteed a vacancy here
        var vacancy = vacancies.First(x => x.VacancyReference == source.VacancyReference);

        if (vacancy.TrainingProvider?.Ukprn is null)
        {
            logger.LogWarning("Failed to migrate '{ApplicationReviewId}' due to bad Ukprn value '{sourceValue}'", source.Id, vacancy.TrainingProvider?.Ukprn);
            return SqlApplicationReview.None;
        }

        // currently TryDecode throws if null/"" is passed :/
        if (string.IsNullOrWhiteSpace(vacancy.EmployerAccountId) || !encodingService.TryDecode(vacancy.EmployerAccountId, EncodingType.AccountId, out var accountId))
        {
            logger.LogWarning("Failed to migrate '{ApplicationReviewId}' due to bad EmployerAccountId value '{sourceValue}'", source.Id, vacancy.EmployerAccountId);
            return SqlApplicationReview.None;
        }

        if (string.IsNullOrWhiteSpace(vacancy.AccountLegalEntityPublicHashedId) || !encodingService.TryDecode(vacancy.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId, out var accountLegalEntityId))
        {
            logger.LogWarning("Failed to migrate '{ApplicationReviewId}' due to bad AccountLegalEntityPublicHashedId value '{sourceValue}'", source.Id, vacancy.AccountLegalEntityPublicHashedId);
            return SqlApplicationReview.None;
        }

        return new SqlApplicationReview
        {
            AccountId = accountId,
            AccountLegalEntityId = accountLegalEntityId,
            AdditionalQuestion1 = source.Application?.AdditionalQuestion1Text,
            AdditionalQuestion2 = source.Application?.AdditionalQuestion2Text,
            ApplicationId = source.Application?.IsFaaV2Application is true ? source.Application?.ApplicationId : null,
            CandidateFeedback = source.CandidateFeedback,
            CandidateId = source.CandidateId,
            CreatedDate = source.CreatedDate,
            DateSharedWithEmployer = source.DateSharedWithEmployer,
            EmployerFeedback = source.EmployerFeedback,
            HasEverBeenEmployerInterviewing = source.HasEverBeenEmployerInterviewing ?? false,
            Id = source.Id,
            LegacyApplicationId = source.Application?.IsFaaV2Application is false ? null : source.Id,
            ReviewedDate = source.ReviewedDate,
            Status = MapStatus(source.Status),
            StatusUpdatedDate = source.StatusUpdatedDate,
            SubmittedDate = source.SubmittedDate,
            Ukprn = (int)vacancy.TrainingProvider.Ukprn,
            VacancyReference = source.VacancyReference,
            VacancyTitle = vacancy.Title ?? string.Empty,
            WithdrawnDate = source.WithdrawnDate
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