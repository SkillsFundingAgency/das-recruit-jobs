using System.Text.Json.Serialization;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class Vacancy
{
    internal static readonly Vacancy None = new Vacancy
    {
        Status = VacancyStatus.Draft,
        ReviewCount = 0,
    };
    
    public Guid Id {get; set;}	                                //  uniqueidentifier    NOT NULL default NEWSEQUENTIALID(),
    public long? VacancyReference {get; set;}                   //  bigint              NULL, -- default next value for VacancyReference,
    public long? AccountId {get; set;}                          //  bigint              NULL, -- converted from EmployerAccountId e.g. MEZSGQ
    public required VacancyStatus Status {get; set;}            //  nvarchar(20)        NOT NULL default 'Draft', -- max is currently 9 chars
    public ApprenticeshipTypes? ApprenticeshipType {get; set;}  //  nvarchar(30)        NOT NULL default 'Standard', -- max is currently 10 chars
    public string? Title {get; set;}                            //  nvarchar(200)       NULL, -- validation max current is 100
    public OwnerType? OwnerType {get; set;}                     //  nvarchar(20)        NOT NULL, -- max is currently 8 chars
    public SourceOrigin? SourceOrigin {get; set;}               //  nvarchar(20)        NULL, -- max is currently 12 chars
    public SourceType? SourceType {get; set;}                   //  nvarchar(20)        NULL, -- max is currently 9 chars
    public long? SourceVacancyReference {get; set;}             //  bigint              NULL,
    public DateTime? ApprovedDate {get; set;}                   //  datetime            NULL,
    public DateTime? CreatedDate {get; set;}                    //  datetime            NOT NULL default CURRENT_TIMESTAMP,
    public DateTime? LastUpdatedDate {get; set;}                //  datetime            NULL,
    public DateTime? SubmittedDate {get; set;}                  //  datetime            NULL,
    public DateTime? ClosedDate {get; set;}                     //  datetime            NULL,
    public DateTime? DeletedDate {get; set;}                    //  datetime            NULL,
    public DateTime? LiveDate {get; set;}                       //  datetime            NULL,
    public DateTime? StartDate {get; set;}                      //  datetime            NULL,
    public DateTime? ClosingDate {get; set;}                    //  datetime            NULL,
    public int ReviewCount {get; set;}                          //  int                 NOT NULL default 0,
    public string? ApplicationUrl {get; set;}                   //  nvarchar(500)       NULL, -- validation currently allows 2000
    public ApplicationMethod? ApplicationMethod {get; set;}     //  nvarchar(50)        NULL, -- max is currently 30 chars
    public string? ApplicationInstructions {get; set;}          //  nvarchar(500)       NULL,
    public string? ShortDescription {get; set;}                 //  nvarchar(350)       NULL,
    public string? Description {get; set;}                      //  nvarchar(max)       NULL,
    public string? AnonymousReason {get; set;}                  //  nvarchar(200)       NULL,
    public bool? DisabilityConfident {get; set;}                //  bit                 NULL,
    public string? ContactName {get; set;}                      //  nvarchar(100)       NULL,
    public string? ContactEmail {get; set;}                     //  nvarchar(100)       NULL,
    public string? ContactPhone {get; set;}                     //  nvarchar(20)        NULL,
    public string? EmployerDescription {get; set;}              //  nvarchar(max)       NULL,
    public string? EmployerLocations {get; set;}                //  nvarchar(max)       NULL,
    public AvailableWhere? EmployerLocationOption {get; set;}   //  nvarchar(30)        NULL, -- max is currently 17 chars
    public string? EmployerLocationInformation {get; set;}      //  nvarchar(500)       NULL,
    public string? EmployerName {get; set;}                     //  nvarchar(100)       NULL,
    public EmployerNameOption? EmployerNameOption {get; set;}   //  nvarchar(30)        NULL, -- max is currently 14 chars
    public string? EmployerRejectedReason {get; set;}           //  nvarchar(200)       NULL,
    public string? LegalEntityName {get; set;}                  //  nvarchar(100)       NULL,
    public string? EmployerWebsiteUrl {get; set;}               //  nvarchar(100)       NULL,
    public GeoCodeMethod? GeoCodeMethod {get; set;}             //  nvarchar(30)        NULL, -- max is currently 18 chars
    public long? AccountLegalEntityId {get; set;}               //  bigint              NULL,
    public int? NumberOfPositions {get; set;}                   //  int                 NULL,
    public string? OutcomeDescription {get; set;}               //  nvarchar(max)       NULL,
    public string? ProgrammeId {get; set;}                      //  nvarchar(20)        NULL,
    public string? Skills {get; set;}                           //  nvarchar(max)       NULL, -- json serialised
    public string? Qualifications {get; set;}                   //  nvarchar(max)       NULL, -- json serialised
    public string? ThingsToConsider {get; set;}                 //  nvarchar(max)       NULL,
    public string? TrainingDescription {get; set;}              //  nvarchar(max)       NULL,
    public string? AdditionalTrainingDescription {get; set;}    //  nvarchar(max)       NULL,
    public int? Ukprn {get; set;}                               //  int                 NULL,
    public string? TrainingProvider_Name {get; set;}            //  nvarchar(100)       NULL,
    public string? TrainingProvider_Address {get; set;}         //  nvarchar(500)       NULL,
    public int? Wage_Duration {get; set;}                       //  int                 NULL,
    public DurationUnit? Wage_DurationUnit {get; set;}          //  nvarchar(10)        NULL,
    public string? Wage_WorkingWeekDescription {get; set;}      //  nvarchar(250)       NULL,
    public decimal? Wage_WeeklyHours {get; set;}                //  decimal             NULL,
    public WageType? Wage_WageType {get; set;}                  //  nvarchar(30)        NULL, -- max is currently 28 chars
    public decimal? Wage_FixedWageYearlyAmount {get; set;}      //  decimal             NULL,
    public string? Wage_WageAdditionalInformation {get; set;}   //  nvarchar(250)       NULL,
    public string? Wage_CompanyBenefitsInformation {get; set;}  //  nvarchar(250)       NULL,
    public ClosureReason? ClosureReason {get; set;}             //  nvarchar(30)        NULL, -- max is currently 21 chars
    public string? TransferInfo {get; set;}                     //  nvarchar(500)       NULL, -- json serialise
    public string? AdditionalQuestion1 {get; set;}              //  nvarchar(250)       NULL,
    public string? AdditionalQuestion2 {get; set;}              //  nvarchar(250)       NULL,
    public bool? HasSubmittedAdditionalQuestions {get; set;}    //  bit                 NULL,
    public bool? HasChosenProviderContactDetails {get; set;}    //  bit                 NULL,
    public bool? HasOptedToAddQualifications {get; set;}        //  bit                 NULL,
    public string? EmployerReviewFieldIndicators {get; set;}    //  nvarchar(max)       NULL, -- json serialised
    public string? ProviderReviewFieldIndicators {get; set;}    //  nvarchar(max)       NULL, -- json serialised
    public Guid? SubmittedByUserId { get; set; }                //  uniqueidentifier    NULL,
    public Guid? ReviewRequestedByUserId { get; set; }           //  uniqueidentifier    NULL,
    public DateTime? ReviewRequestedDate { get; set; }
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
    Approved
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