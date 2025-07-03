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
public class VacancyMapper(ILogger<VacancyMapper> logger, IEncodingService encodingService, UserLookupService userLookupService)
{
    public async Task<SqlVacancy> MapFromAsync(MongoVacancy vacancy)
    {
        var createdByUserId = await userLookupService.LookupUserAsync(vacancy.CreatedByUser);
        if (createdByUserId == User.None)
        {
            logger.LogWarning("Failed to migrate vacancy '{VacancyId}' due to bad or unknown CreatedByUser", vacancy.Id);
            return SqlVacancy.None;
        }
        
        var submittedByUserId = await userLookupService.LookupUserAsync(vacancy.SubmittedByUser);
        if (submittedByUserId == User.None && vacancy.SubmittedDate is null)
        {
            logger.LogWarning("Failed to migrate vacancy '{VacancyId}' due to bad or unknown SubmittedByUser", vacancy.Id);
            return SqlVacancy.None;
        }
        
        long? accountId = null;
        if (!string.IsNullOrWhiteSpace(vacancy.EmployerAccountId))
        {
            try
            {
                // currently TryDecode throws if null is passed :/
                encodingService.TryDecode(vacancy.EmployerAccountId, EncodingType.AccountId, out var accountIdValue);
                accountId = accountIdValue;
            }
            catch (Exception e)
            {
                logger.LogWarning("Failed to migrate vacancy '{VacancyId}' due to bad EmployerAccountId value '{sourceValue}'", vacancy.Id, vacancy.EmployerAccountId);
                return SqlVacancy.None;
            }
        }

        long? accountLegalEntityId = null;
        if (!string.IsNullOrWhiteSpace(vacancy.AccountLegalEntityPublicHashedId))
        {
            try
            {
                // currently TryDecode throws if null is passed :/
                encodingService.TryDecode(vacancy.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId, out var accountLegalEntityIdValue);
                accountLegalEntityId = accountLegalEntityIdValue;
            }
            catch (Exception e)
            {
                logger.LogWarning("Failed to migrate vacancy '{VacancyId}' due to bad AccountLegalEntityPublicHashedId value '{sourceValue}'", vacancy.Id, vacancy.AccountLegalEntityPublicHashedId);
                return SqlVacancy.None;
            }
        }

        // TODO: sort out contacts
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
            locations = MigrationUtils.Serialize(new List<MongoAddress> { vacancy.EmployerLocation });
            locationOption = AvailableWhere.OneLocation;
        }
        else if (vacancy.EmployerLocations is { Count: >0 })
        {
            locations = MigrationUtils.Serialize(vacancy.EmployerLocations);
            locationOption = Enum.Parse<AvailableWhere>(vacancy.EmployerLocationOption!.Value.ToString());
        }

        string? employerReviewFieldIndicators = null;
        if (vacancy.EmployerReviewFieldIndicators is not null)
        {
            employerReviewFieldIndicators = MigrationUtils.Serialize(vacancy.EmployerReviewFieldIndicators);
        }
        
        string? providerReviewFieldIndicators = null;
        if (vacancy.ProviderReviewFieldIndicators is not null)
        {
            providerReviewFieldIndicators = MigrationUtils.Serialize(vacancy.ProviderReviewFieldIndicators);
        }
        
        string? skills = null;
        if (vacancy.Skills is not null)
        {
            skills = MigrationUtils.Serialize(vacancy.Skills);
        }
        
        string? qualifications = null;
        if (vacancy.Qualifications is not null)
        {
            qualifications = MigrationUtils.Serialize(vacancy.Qualifications);
        }
        
        string? transferInfo = null;
        if (vacancy.TransferInfo is not null)
        {
            transferInfo = MigrationUtils.Serialize(vacancy.TransferInfo);
        }
        
        string? trainingProviderAddress = null;
        if (vacancy.TrainingProvider?.Address is not null)
        {
            trainingProviderAddress = MigrationUtils.Serialize(vacancy.TrainingProvider?.Address);
        }

        return new SqlVacancy
        {
            Id = vacancy.Id,
            VacancyReference = vacancy.VacancyReference,
            AccountId = accountId,
            Status = Enum.Parse<VacancyStatus>(vacancy.Status.ToString()),
            ApprenticeshipType = vacancy.ApprenticeshipType is not null ? Enum.Parse<ApprenticeshipTypes>(vacancy.ApprenticeshipType!.Value.ToString()) : ApprenticeshipTypes.Standard,
            Title = vacancy.Title,
            OwnerType = Enum.Parse<OwnerType>(vacancy.OwnerType.ToString()),
            SourceOrigin = Enum.Parse<SourceOrigin>(vacancy.SourceOrigin.ToString()),
            SourceType = Enum.Parse<SourceType>(vacancy.SourceType.ToString()),
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
            ApplicationMethod = vacancy.ApplicationMethod is not null ? Enum.Parse<ApplicationMethod>(vacancy.ApplicationMethod!.Value.ToString()) : null,
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
            EmployerNameOption = vacancy.EmployerNameOption is not null ? Enum.Parse<EmployerNameOption>(vacancy.EmployerNameOption!.Value.ToString()) : null,
            EmployerRejectedReason = vacancy.EmployerRejectedReason,
            LegalEntityName = vacancy.LegalEntityName,
            EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
            GeoCodeMethod = vacancy.GeoCodeMethod is not null ? Enum.Parse<GeoCodeMethod>(vacancy.GeoCodeMethod!.Value.ToString()) : null,
            AccountLegalEntityId = accountLegalEntityId,
            NumberOfPositions = vacancy.NumberOfPositions,
            OutcomeDescription = vacancy.OutcomeDescription,
            ProgrammeId = vacancy.ProgrammeId,
            Skills = skills,
            Qualifications = qualifications,
            ThingsToConsider = vacancy.ThingsToConsider,
            TrainingDescription = vacancy.TrainingDescription,
            AdditionalTrainingDescription = vacancy.AdditionalTrainingDescription,
            Ukprn = (int?)vacancy.TrainingProvider?.Ukprn,
            TrainingProvider_Name = vacancy.TrainingProvider?.Name,
            TrainingProvider_Address = trainingProviderAddress,
            Wage_Duration = vacancy.Wage?.Duration,
            Wage_DurationUnit = vacancy.Wage?.DurationUnit is not null ? Enum.Parse<DurationUnit>(vacancy.Wage.DurationUnit.Value.ToString()) : null,
            Wage_WorkingWeekDescription = vacancy.Wage?.WorkingWeekDescription,
            Wage_WeeklyHours = vacancy.Wage?.WeeklyHours,
            Wage_WageType = vacancy.Wage?.WageType is not null ? Enum.Parse<WageType>(vacancy.Wage.WageType.Value.ToString()) : null,
            Wage_FixedWageYearlyAmount = vacancy.Wage?.FixedWageYearlyAmount,
            Wage_WageAdditionalInformation = vacancy.Wage?.WageAdditionalInformation,
            Wage_CompanyBenefitsInformation = vacancy.Wage?.CompanyBenefitsInformation,
            ClosureReason = vacancy.ClosureReason is not null ? Enum.Parse<ClosureReason>(vacancy.ClosureReason!.Value.ToString()) : null,
            TransferInfo = transferInfo,
            AdditionalQuestion1 = vacancy.AdditionalQuestion1,
            AdditionalQuestion2 = vacancy.AdditionalQuestion2,
            HasSubmittedAdditionalQuestions = vacancy.HasSubmittedAdditionalQuestions,
            HasChosenProviderContactDetails = vacancy.HasChosenProviderContactDetails,
            HasOptedToAddQualifications = vacancy.HasOptedToAddQualifications,
            EmployerReviewFieldIndicators = employerReviewFieldIndicators,
            ProviderReviewFieldIndicators = providerReviewFieldIndicators,
        };
    }
}