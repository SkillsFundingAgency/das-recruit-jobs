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

    [Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData requestData)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await vacancyAnalyticsMigrationStrategy.RunAsync();
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