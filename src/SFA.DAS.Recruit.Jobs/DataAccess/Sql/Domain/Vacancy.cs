using System.Text.Json.Serialization;
using Address = SFA.DAS.Recruit.Jobs.Domain.Address;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class Vacancy
{
    internal static readonly Vacancy None = new Vacancy
    {
        Status = VacancyStatus.Draft,
        ReviewCount = 0,
    };

    public Guid Id { get; set; }
    public long? VacancyReference { get; set; }
    public long? AccountId { get; set; }
    public required VacancyStatus Status { get; set; }
    public ApprenticeshipTypes? ApprenticeshipType { get; set; }
    public string? Title { get; set; }
    public OwnerType? OwnerType { get; set; }
    public SourceOrigin? SourceOrigin { get; set; }
    public SourceType? SourceType { get; set; }
    public ArchiveType? ArchiveType { get; set; }
    public long? SourceVacancyReference { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ReviewRequestedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
    public DateTime? LiveDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public DateTime? ArchivedDate { get; set; }
    public int ReviewCount { get; set; }
    public string? ApplicationUrl { get; set; }
    public ApplicationMethod? ApplicationMethod { get; set; }
    public string? ApplicationInstructions { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? AnonymousReason { get; set; }
    public bool? DisabilityConfident { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? EmployerDescription { get; set; }
    public List<Address>? EmployerLocations { get; set; }
    public AvailableWhere? EmployerLocationOption { get; set; }
    public string? EmployerLocationInformation { get; set; }
    public string? EmployerName { get; set; }
    public EmployerNameOption? EmployerNameOption { get; set; }
    public string? EmployerRejectedReason { get; set; }
    public string? LegalEntityName { get; set; }
    public string? EmployerWebsiteUrl { get; set; }
    public GeoCodeMethod? GeoCodeMethod { get; set; }
    public long? AccountLegalEntityId { get; set; }
    public int? NumberOfPositions { get; set; }
    public string? OutcomeDescription { get; set; }
    public string? ProgrammeId { get; set; }
    public string? Skills { get; set; }
    public List<Qualification>? Qualifications { get; set; }
    public string? ThingsToConsider { get; set; }
    public string? TrainingDescription { get; set; }
    public string? AdditionalTrainingDescription { get; set; }
    public int? Ukprn { get; set; }
    public string? TrainingProvider_Name { get; set; }
    public string? TrainingProvider_Address { get; set; }
    public int? Wage_Duration { get; set; }
    public DurationUnit? Wage_DurationUnit { get; set; }
    public string? Wage_WorkingWeekDescription { get; set; }
    public decimal? Wage_WeeklyHours { get; set; }
    public WageType? Wage_WageType { get; set; }
    public decimal? Wage_FixedWageYearlyAmount { get; set; }
    public string? Wage_WageAdditionalInformation { get; set; }
    public string? Wage_CompanyBenefitsInformation { get; set; }
    public ClosureReason? ClosureReason { get; set; }
    public string? TransferInfo { get; set; }
    public string? AdditionalQuestion1 { get; set; }
    public string? AdditionalQuestion2 { get; set; }
    public bool? HasSubmittedAdditionalQuestions { get; set; }
    public bool? HasChosenProviderContactDetails { get; set; }
    public bool? HasOptedToAddQualifications { get; set; }
    public string? EmployerReviewFieldIndicators { get; set; }
    public string? ProviderReviewFieldIndicators { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public Guid? ReviewRequestedByUserId { get; set; }
    public Guid? ArchivedByUserId { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApprenticeshipTypes
{
    Standard,
    Foundation,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VacancyType
{
    Apprenticeship = 0,
    Traineeship = 1
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VacancyStatus
{
    Draft,
    Review,
    Rejected,
    Submitted,
    Referred,
    Live,
    Closed,
    Approved,
    Archived
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClosureReason
{
    Auto,
    Manual,
    TransferredByQa,
    BlockedByQa,
    TransferredByEmployer,
    WithdrawnByQa
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransferReason
{
    EmployerRevokedPermission,
    BlockedByQa
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WageType
{
    Unspecified,
    FixedWage,
    NationalMinimumWageForApprentices,
    NationalMinimumWage,
    CompetitiveSalary
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DurationUnit
{
    Week,
    Month,
    Year
}

[JsonConverter(typeof(JsonStringEnumConverter))]
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

[JsonConverter(typeof(JsonStringEnumConverter))]
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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DisabilityConfident
{
    No = 0,
    Yes
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApplicationMethod
{
    ThroughFindAnApprenticeship,
    ThroughExternalApplicationSite,
    ThroughFindATraineeship
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SourceType
{
    Clone,
    Extension,
    New
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SourceOrigin
{
    Api,
    EmployerWeb,
    ProviderWeb,
    WebComplaint
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OwnerType
{
    Employer = 0,
    Provider = 1,
    External = 2,
    Unknown = 3
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ArchiveType
{
    Auto = 0,
    Manual = 1
}

public class Qualification
{
    public string? QualificationType { get; set; }
    public string? Subject { get; set; }
    public string? Grade { get; set; }
    public int? Level { get; set; }
    public QualificationWeighting? Weighting { get; set; }
    public string? OtherQualificationName { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QualificationWeighting
{
    Essential,
    Desired
}