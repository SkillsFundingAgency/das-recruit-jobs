using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMigration;

[ExcludeFromCodeCoverage]
public class VacancyMigrationHttpTrigger(
    ILogger<VacancyMigrationHttpTrigger> logger,
    VacancyMigrationStrategy vacancyMigrationStrategy)
{
    private const string TriggerName = nameof(VacancyMigrationHttpTrigger);
    
    [Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData requestData)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            var request = await JsonSerializer.DeserializeAsync<MigrateVacanciesHttpRequest>(requestData.Body);
            if (request?.VacancyIds is { Count: > 0 })
            {
                await vacancyMigrationStrategy.RunAsync(request.VacancyIds);
            }
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