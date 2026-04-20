namespace SFA.DAS.Recruit.Jobs.NServiceBus.Commands;

public sealed record PublishVacancyCommand(Guid VacancyId) : ICommand;