using SFA.DAS.Recruit.Jobs.Features.VacancyPublishing.Handlers;
using SFA.DAS.Recruit.Jobs.NServiceBus.Commands;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyPublishing.CommandHandlers;

public class PublishVacancyCommandHandler(IPublishVacancyHandler handler) : IHandleMessages<PublishVacancyCommand>
{
    public async Task Handle(PublishVacancyCommand message, IMessageHandlerContext context)
    {
        await handler.HandleAsync(message.VacancyId, context.CancellationToken);
    }
}