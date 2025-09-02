using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.Core;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using MongoAddress = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.Address;
using DisabilityConfident = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.DisabilityConfident;
using SqlVacancy = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.Vacancy;
using MongoVacancy = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.Vacancy;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

[ExcludeFromCodeCoverage]
public class VacancyMapper(ILogger<VacancyMapper> logger, IEncodingService encodingService, UserLocator userLocator)
{
    public async Task<SqlVacancy> MapFromAsync(MongoVacancy vacancy)
    {
        var submittedByUserId = await userLocator.LocateMongoUserAsync(vacancy.SubmittedByUser);
        if (vacancy.SubmittedByUser is not null && submittedByUserId is null)
        {
            logger.LogWarning("Failed to lookup submitting user for vacancy '{vacancyReference}'", vacancy.VacancyReference);
        }

        var reviewRequestedBy = await userLocator.LocateMongoUserAsync(vacancy.ReviewByUser);
        
        if (vacancy.Status is DataAccess.MongoDb.Domain.VacancyStatus.Closed && !int.TryParse(vacancy.ProgrammeId, out var programmeId))
        {
            logger.LogWarning("Failed to parse ProgrammeId '{ProgrammeId}' for vacancy '{VacancyReference}', could be a Framework code", vacancy.ProgrammeId, vacancy.VacancyReference);
            return SqlVacancy.None;
        }
        
        long? accountId = encodingService.TryDecodeAccountId(vacancy.EmployerAccountId, out var accountIdValue) ? accountIdValue : null;
        long? accountLegalEntityId = encodingService.TryDecodePublicAccountLegalEntityId(vacancy.AccountLegalEntityPublicHashedId, out var accountLegalEntityIdValue) ? accountLegalEntityIdValue : null;

        string? contactName = null;
        string? contactEmail = null;
        string? contactPhone = null;
        if (vacancy.EmployerContact is not null)
        {
            contactName = vacancy.EmployerContact.Name;
            contactEmail = vacancy.EmployerContact.Email;
            contactPhone = vacancy.EmployerContact.Phone;
        } 
        else if (vacancy.ProviderContact is not null)
        {
            contactName = vacancy.ProviderContact.Name;
            contactEmail = vacancy.ProviderContact.Email;
            contactPhone = vacancy.ProviderContact.Phone;
        }

        var locations = string.Empty;
        AvailableWhere? locationOption = null;
        if (vacancy.EmployerLocation is not null)
        {
            locations = MigrationUtils.SerializeOrNull(new List<MongoAddress> { vacancy.EmployerLocation });
            locationOption = AvailableWhere.OneLocation;
        }
        else if (vacancy.EmployerLocations is { Count: >0 })
        {
            locations = MigrationUtils.SerializeOrNull(vacancy.EmployerLocations);
            locationOption = Enum.Parse<AvailableWhere>(vacancy.EmployerLocationOption!.Value.ToString());
        }

        return new SqlVacancy
        {
            Id = vacancy.Id,
            VacancyReference = vacancy.VacancyReference,
            AccountId = accountId,
            Status = MigrationUtils.ParseEnumIfNotNull<VacancyStatus>(vacancy.Status.ToString())!.Value,
            ApprenticeshipType = MigrationUtils.ParseEnumIfNotNull<ApprenticeshipTypes>(vacancy.ApprenticeshipType?.ToString()) ?? ApprenticeshipTypes.Standard,
            Title = vacancy.Title,
            OwnerType = MigrationUtils.ParseEnumIfNotNull<OwnerType>(vacancy.OwnerType.ToString()),
            SourceOrigin = MigrationUtils.ParseEnumIfNotNull<SourceOrigin>(vacancy.SourceOrigin.ToString()),
            SourceType = MigrationUtils.ParseEnumIfNotNull<SourceType>(vacancy.SourceType.ToString()),
            SourceVacancyReference = vacancy.SourceVacancyReference,
            ApprovedDate = vacancy.ApprovedDate,
            CreatedDate = vacancy.CreatedDate,
            LastUpdatedDate = vacancy.LastUpdatedDate,
            SubmittedDate = vacancy.SubmittedDate,
            ReviewDate = vacancy.ReviewDate,
            ClosedDate = vacancy.ClosedDate,
            DeletedDate = vacancy.DeletedDate,
            LiveDate = vacancy.LiveDate,
            StartDate = vacancy.StartDate,
            ClosingDate = vacancy.ClosingDate,
            ReviewCount = vacancy.ReviewCount,
            ApplicationUrl = vacancy.ApplicationUrl,
            ApplicationMethod = MigrationUtils.ParseEnumIfNotNull<ApplicationMethod>(vacancy.ApplicationMethod?.ToString()),
            ApplicationInstructions = vacancy.ApplicationInstructions,
            ShortDescription = vacancy.ShortDescription,
            Description = vacancy.Description,
            AnonymousReason = vacancy.AnonymousReason,
            DisabilityConfident = vacancy.DisabilityConfident == DisabilityConfident.Yes,
            ContactName = contactName,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            EmployerDescription = vacancy.EmployerDescription,
            EmployerLocations = locations,
            EmployerLocationOption = locationOption,
            EmployerLocationInformation = vacancy.EmployerLocationInformation,
            EmployerName = vacancy.EmployerName,
            EmployerNameOption = MigrationUtils.ParseEnumIfNotNull<EmployerNameOption>(vacancy.EmployerNameOption?.ToString()),
            EmployerRejectedReason = vacancy.EmployerRejectedReason,
            LegalEntityName = vacancy.LegalEntityName,
            EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
            GeoCodeMethod = MigrationUtils.ParseEnumIfNotNull<GeoCodeMethod>(vacancy.GeoCodeMethod?.ToString()),
            AccountLegalEntityId = accountLegalEntityId,
            NumberOfPositions = vacancy.NumberOfPositions,
            OutcomeDescription = vacancy.OutcomeDescription,
            ProgrammeId = vacancy.ProgrammeId,
            Skills = MigrationUtils.SerializeOrNull(vacancy.Skills),
            Qualifications = MigrationUtils.SerializeOrNull(vacancy.Qualifications),
            ThingsToConsider = vacancy.ThingsToConsider,
            TrainingDescription = vacancy.TrainingDescription,
            AdditionalTrainingDescription = vacancy.AdditionalTrainingDescription,
            Ukprn = (int?)vacancy.TrainingProvider?.Ukprn,
            TrainingProvider_Name = vacancy.TrainingProvider?.Name,
            TrainingProvider_Address = MigrationUtils.SerializeOrNull(vacancy.TrainingProvider?.Address),
            Wage_Duration = vacancy.Wage?.Duration,
            Wage_DurationUnit = MigrationUtils.ParseEnumIfNotNull<DurationUnit>(vacancy.Wage?.DurationUnit.ToString()),
            Wage_WorkingWeekDescription = vacancy.Wage?.WorkingWeekDescription,
            Wage_WeeklyHours = vacancy.Wage?.WeeklyHours,
            Wage_WageType = MigrationUtils.ParseEnumIfNotNull<WageType>(vacancy.Wage?.WageType.ToString()),
            Wage_FixedWageYearlyAmount = vacancy.Wage?.FixedWageYearlyAmount,
            Wage_WageAdditionalInformation = vacancy.Wage?.WageAdditionalInformation,
            Wage_CompanyBenefitsInformation = vacancy.Wage?.CompanyBenefitsInformation,
            ClosureReason = MigrationUtils.ParseEnumIfNotNull<ClosureReason>(vacancy.ClosureReason?.ToString()),
            TransferInfo = MigrationUtils.SerializeOrNull(vacancy.TransferInfo),
            AdditionalQuestion1 = vacancy.AdditionalQuestion1,
            AdditionalQuestion2 = vacancy.AdditionalQuestion2,
            HasSubmittedAdditionalQuestions = vacancy.HasSubmittedAdditionalQuestions,
            HasChosenProviderContactDetails = vacancy.HasChosenProviderContactDetails,
            HasOptedToAddQualifications = vacancy.HasOptedToAddQualifications,
            EmployerReviewFieldIndicators = MigrationUtils.SerializeOrNull(vacancy.EmployerReviewFieldIndicators),
            ProviderReviewFieldIndicators = MigrationUtils.SerializeOrNull(vacancy.ProviderReviewFieldIndicators),
            SubmittedByUserId = submittedByUserId,
            ReviewRequestedByUserId = reviewRequestedBy,
            ReviewRequestedDate = vacancy.ReviewDate
        };
    }
}