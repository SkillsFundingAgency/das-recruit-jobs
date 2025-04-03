using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

[ExcludeFromCodeCoverage]
public class ApplicationReview
{
    public static readonly ApplicationReview None = new ()
    {
        Status = ApplicationReviewStatus.New,
        CreatedDate = default,
        SubmittedDate = default,
        CandidateId = Guid.Empty,
        Id = Guid.Empty,
        Owner = Owner.Provider,
        Ukprn = 0,
        AccountId = 0,
        AccountLegalEntityId = 0,
        VacancyReference = 0,
        VacancyTitle = string.Empty
    };

    public DateTime? DateSharedWithEmployer { get; init; }
    public DateTime? ReviewedDate { get; init; }
    public DateTime? StatusUpdatedDate { get; init; }
    public DateTime? WithdrawnDate { get; init; }
    public Guid? ApplicationId { get; init; }
    public Guid? LegacyApplicationId { get; init; }
    public bool HasEverBeenEmployerInterviewing { get; init; }
    public required ApplicationReviewStatus Status { get; init; }
    public required DateTime CreatedDate { get; init; }
    public required DateTime SubmittedDate { get; init; }
    public required Guid CandidateId { get; init; }
    public required Guid Id { get; init; }
    public required Owner Owner { get; init; }
    public required int Ukprn { get; init; }
    public required long AccountId { get; init; }
    public required long AccountLegalEntityId { get; init; }
    public required long VacancyReference  { get; init; }
    public required string VacancyTitle { get; init; }
    public string? AdditionalQuestion1 { get; init; }
    public string? AdditionalQuestion2 { get; init; }
    public string? CandidateFeedback { get; init; }
    public string? EmployerFeedback { get; init; }
}