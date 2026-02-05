using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyAnalyticsMigration;
[ExcludeFromCodeCoverage]
public class VacancyAnalyticsMigrationHttpTrigger(ILogger<VacancyAnalyticsMigrationHttpTrigger> logger,
    VacancyAnalyticsMigrationStrategy vacancyAnalyticsMigrationStrategy)
{
    private const string TriggerName = nameof(VacancyAnalyticsMigrationHttpTrigger);

    // Function attribute is commented out to prevent accidental execution of the migration. To enable the migration, uncomment the Function attribute and ensure the timer trigger schedule is appropriate for your needs.
    //[Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData requestData, CancellationToken cancellationToken)
    {

        return;

        // This trigger is intended to be used as a backup to the timer trigger, allowing the migration to be manually triggered if required. It is not intended to be used in production and should only be enabled if required.
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await vacancyAnalyticsMigrationStrategy.RunAsync(cancellationToken);
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