using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.BlockedOrganisationsMigration;

[ExcludeFromCodeCoverage]
public class BlockOrganisationMigrationTimerTrigger(ILogger<BlockOrganisationMigrationTimerTrigger> logger, BlockedOrganisationMigrationStrategy migrationStrategy)
{
    private const string TriggerName = nameof(BlockOrganisationMigrationTimerTrigger);
    
    [Function(TriggerName)]
    public async Task Run([TimerTrigger("*/5 23-23 * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await migrationStrategy.RunAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during migration", TriggerName);
            throw;
        }
        logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
    }
}