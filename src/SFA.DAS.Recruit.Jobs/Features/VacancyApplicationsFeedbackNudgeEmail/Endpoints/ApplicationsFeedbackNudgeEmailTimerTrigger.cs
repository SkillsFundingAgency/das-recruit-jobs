using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Core;
using SFA.DAS.Recruit.Jobs.Features.VacancyApplicationsFeedbackNudgeEmail.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyApplicationsFeedbackNudgeEmail.Endpoints;

public class ApplicationsFeedbackNudgeEmailTimerTrigger(ILogger<ApplicationsFeedbackNudgeEmailTimerTrigger> logger, IApplicationsFeedbackNudgeEmailHandler handler)
{
    private const string TriggerName = nameof(ApplicationsFeedbackNudgeEmailTimerTrigger);

    [Function(TriggerName)]
    public async Task Run([TimerTrigger(Schedules.FourAmDaily)] TimerInfo _, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        await handler.RunAsync(cancellationToken);
        logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
    }
}