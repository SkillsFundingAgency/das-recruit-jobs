using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

[ExcludeFromCodeCoverage]
public class TrainingProvider
{
    public long? Ukprn { get; init; }
    public string? Name { get; init; }
    public Address? Address { get; init; }
}

public class Vacancy
{
    private string _shortDescription;
    public Guid Id { get; set; }
    public string EmployerAccountId { get; set; }
    public long? VacancyReference { get; set; }
    public VacancyStatus Status { get; set; }
    public ApprenticeshipTypes? ApprenticeshipType { get; set; }
    public OwnerType OwnerType { get; set; }
    public SourceOrigin SourceOrigin { get; set; }
    public SourceType SourceType { get; set; }
    public long? SourceVacancyReference { get; set; }
    public DateTime? ClosedDate { get; set; }
    public VacancyUser ClosedByUser { get; set; }
    public DateTime? CreatedDate { get; set; }
    public VacancyUser CreatedByUser { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public VacancyUser SubmittedByUser { get; set; }
    public DateTime? ReviewDate { get; set; }
    public VacancyUser ReviewByUser { get; set; }
    public int ReviewCount { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? LiveDate { get; set; }

    public DateTime? LastUpdatedDate { get; set; }
    public VacancyUser LastUpdatedByUser { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDate { get; set; }
    public VacancyUser DeletedByUser { get; set; }
    public string AnonymousReason { get; set; }
    public string ApplicationInstructions { get; set; }
    public ApplicationMethod? ApplicationMethod { get; set; }
    public string ApplicationUrl { get; set; }
    public DateTime? ClosingDate { get; set; }
    public string Description { get; set; }
    public DisabilityConfident DisabilityConfident { get; set; }
    public ContactDetail EmployerContact { get; set; }
    public string EmployerDescription { get; set; }
    public Address? EmployerLocation { get; set; }
    public List<Address> EmployerLocations { get; set; }
    public AvailableWhere? EmployerLocationOption { get; set; }
    public string? EmployerLocationInformation { get; set; }
    public string EmployerName { get; set; }
    public EmployerNameOption? EmployerNameOption { get; set; }
    public List<EmployerReviewFieldIndicator> EmployerReviewFieldIndicators { get; set; }
    public string EmployerRejectedReason { get; set; }
    public string LegalEntityName { get; set; }
    public string EmployerWebsiteUrl { get; set; }
    public GeoCodeMethod? GeoCodeMethod { get; set; }
    public string AccountLegalEntityPublicHashedId { get; set; }
    public int? NumberOfPositions { get; set; }
    public string OutcomeDescription { get; set; }
    public ContactDetail ProviderContact { get; set; }
    public List<ProviderReviewFieldIndicator> ProviderReviewFieldIndicators { get; set; }
    public string ProgrammeId { get; set; }
    public bool? HasOptedToAddQualifications { get; set; }
    public List<Qualification> Qualifications { get; set; }

    public string ShortDescription  
    {
        get
        {
            return _shortDescription;
        }
        set
        {
            if (value != null) _shortDescription = value.Replace(Environment.NewLine, " ");
        }
    }

    public List<string> Skills { get; set; }
    public DateTime? StartDate { get; set; }
    public string? ThingsToConsider { get; set; }
    public string? Title { get; set; }
    public string? TrainingDescription { get; set; }
    public string? AdditionalTrainingDescription { get; set; }
    public TrainingProvider TrainingProvider { get; set; }
    public Wage Wage { get; set; }
    public ClosureReason? ClosureReason { get; set; }
    public string ClosureExplanation { get; set; }
    public TransferInfo TransferInfo { get; set; }
    
    public VacancyType? VacancyType { get; set; }

    public string? AdditionalQuestion1 { get; set; }
    public string? AdditionalQuestion2 { get; set; }
    public bool? HasSubmittedAdditionalQuestions { get; set; }
    public bool? HasChosenProviderContactDetails { get; set; }
    
    public DateTime? MigrationDate { get; set; }
    public bool? MigrationFailed { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApprenticeshipTypes
{
    Standard,
    Foundation,
}

public enum VacancyType
{
    Apprenticeship = 0,
    Traineeship = 1
}

public enum VacancyStatus
{
    Draft,
    Review,
    Rejected,
    Submitted,
    Referred,
    Live,
    Closed,
    Approved
}

public enum ClosureReason
{
    Auto,
    Manual,
    TransferredByQa,
    BlockedByQa,
    TransferredByEmployer,
    WithdrawnByQa
}

public class TransferInfo
{
    public long Ukprn { get; set; }
    public string ProviderName { get; set; }
    public string LegalEntityName { get; set; }
    public VacancyUser TransferredByUser { get; set; }
    public DateTime TransferredDate { get; set; }
    public TransferReason Reason { get; set; }
}

public enum TransferReason
{
    EmployerRevokedPermission,
    BlockedByQa
}

public class Wage
{
    public int? Duration { get; set; }

    public DurationUnit? DurationUnit { get; set; }

    public string WorkingWeekDescription { get; set; }

    public decimal? WeeklyHours { get; set; }

    public WageType? WageType { get; set; }

    public decimal? FixedWageYearlyAmount { get; set; }

    public string WageAdditionalInformation { get; set; }
    public string CompanyBenefitsInformation { get; set; }
}

public enum WageType
{
    Unspecified,
    FixedWage,
    NationalMinimumWageForApprentices,
    NationalMinimumWage,
    CompetitiveSalary
}

public enum DurationUnit
{
    Week,
    Month,
    Year
}

public class Qualification : IEquatable<Qualification>

{
    public string QualificationType { get; set; }
    public string Subject { get; set; }
    public string Grade { get; set; }
    public int? Level { get; set; }
    public QualificationWeighting? Weighting { get; set; }
    public string OtherQualificationName { get; set; }
    public bool Equals(Qualification other)
    {
        if (other == null)
            return false;

        return (QualificationType == null || QualificationType.Equals(other.QualificationType)) &&
               (Subject == null || Subject.Equals(other.Subject)) &&
               (Grade == null || Grade.Equals(other.Grade)) &&
               (Weighting == null || Weighting.Equals(other.Weighting)) && 
               (Level == null || Level.Equals(other.Level)) &&
               (OtherQualificationName == null || OtherQualificationName.Equals(other.OtherQualificationName));
    }
}

public enum QualificationWeighting
{
    Essential,
    Desired
}

public class ProviderReviewFieldIndicator
{
    public string FieldIdentifier { get; set; }
    public bool IsChangeRequested { get; set; }
}

public class ContactDetail
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}

public enum GeoCodeMethod
{
    Unspecified,
    ExistingVacancy,
    PostcodesIo,
    Loqate,
    PostcodesIoOutcode,
    OuterApi,
    FailedToGeoCode
}

public class EmployerReviewFieldIndicator
{
    public string FieldIdentifier { get; set; }
    public bool IsChangeRequested { get; set; }
}

public enum EmployerNameOption
{
    RegisteredName,
    TradingName,
    Anonymous
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AvailableWhere
{
    OneLocation,
    MultipleLocations,
    AcrossEngland,
}

public enum DisabilityConfident
{
    No = 0,
    Yes
}

public enum ApplicationMethod
{
    ThroughFindAnApprenticeship,
    ThroughExternalApplicationSite,
    ThroughFindATraineeship
}

public enum SourceType
{
    Clone,
    Extension,
    New
}

public enum SourceOrigin
{
    Api,
    EmployerWeb,
    ProviderWeb,
    WebComplaint
}

public enum OwnerType
{
    Employer = 0,
    Provider = 1,
    External = 2,
    Unknown = 3
}