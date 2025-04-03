using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.ProhibitedContentMigration;

[ExcludeFromCodeCoverage]
public class ProhibitedContentMigrationHttpTrigger(
    ILogger<ProhibitedContentMigrationHttpTrigger> logger,
    ProhibitedContentMigrationStrategy prohibitedContentMigrationStrategy)
{
    private const string TriggerName = nameof(ProhibitedContentMigrationHttpTrigger);
    
    [Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData requestData)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await prohibitedContentMigrationStrategy.RunAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during migration", TriggerName);
            throw;
        }
        finally
        {
            logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
        }
    }
}