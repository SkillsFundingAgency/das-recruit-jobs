using System;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

public class TrainingProvider
{
    public long? Ukprn { get; init; }
}

public class Vacancy
{
    public Guid Id { get; init; }
    public string EmployerAccountId { get; init; } = string.Empty;
    public TrainingProvider? TrainingProvider { get; init; }
    public long VacancyReference { get; init; }
    public string OwnerType { get; set; } = string.Empty;
}

