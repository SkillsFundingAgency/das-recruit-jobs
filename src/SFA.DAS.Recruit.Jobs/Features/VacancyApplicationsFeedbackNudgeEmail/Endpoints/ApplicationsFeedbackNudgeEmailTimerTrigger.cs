using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Recruit.Jobs.Core;
using SFA.DAS.Recruit.Jobs.Features.VacancyApplicationsFeedbackNudgeEmail.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyApplicationsFeedbackNudgeEmail.Endpoints;

public class ApplicationsFeedbackNudgeEmailTimerTrigger(IApplicationsFeedbackNudgeEmailHandler handler)
{
    private const string TriggerName = nameof(ApplicationsFeedbackNudgeEmailTimerTrigger);
    private const int DaysValue = -29; // 4 weeks from yesterday

    [Function(TriggerName)]
    public async Task Run([TimerTrigger(Schedules.FourAmDaily, RunOnStartup = true)] TimerInfo _, CancellationToken cancellationToken)
    {
        var date = DateTime.UtcNow.AddDays(DaysValue);
        await handler.RunAsync(DateOnly.FromDateTime(date), cancellationToken);
    }
}