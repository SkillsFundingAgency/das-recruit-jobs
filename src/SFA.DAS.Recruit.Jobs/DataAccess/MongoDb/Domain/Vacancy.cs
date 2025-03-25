using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

[ExcludeFromCodeCoverage]
public class TrainingProvider
{
    public long? Ukprn { get; init; }
}

public class Vacancy
{
    public Guid Id { get; init; }
    public string AccountLegalEntityPublicHashedId { get; init; } = string.Empty;
    public string EmployerAccountId { get; init; } = string.Empty;
    public string OwnerType { get; set; } = string.Empty;
    public TrainingProvider? TrainingProvider { get; init; }
    public long VacancyReference { get; init; }
    public string? Title { get; set; }
}

