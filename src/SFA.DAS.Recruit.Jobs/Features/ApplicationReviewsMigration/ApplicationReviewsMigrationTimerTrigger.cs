﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

[ExcludeFromCodeCoverage]
public class ApplicationReviewsMigrationTimerTrigger(
    ILogger<ApplicationReviewsMigrationTimerTrigger> logger,
    ApplicationReviewMigrationStrategy applicationReviewMigrationStrategy)
{
    private const string TriggerName = nameof(ApplicationReviewsMigrationTimerTrigger);
    
    [Function(TriggerName)]
    public async Task Run([TimerTrigger("*/5 23-5 * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            //await applicationReviewMigrationStrategy.RunAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during migration", TriggerName);
            throw;
        }
        logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
    }
}