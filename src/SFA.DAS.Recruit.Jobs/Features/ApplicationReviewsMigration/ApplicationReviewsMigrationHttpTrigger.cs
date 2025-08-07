using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

[ExcludeFromCodeCoverage]
public class ApplicationReviewsMigrationHttpTrigger(
    ILogger<ApplicationReviewsMigrationHttpTrigger> logger,
    ApplicationReviewMigrationStrategy applicationReviewMigrationStrategy)
{
    private const string TriggerName = nameof(ApplicationReviewsMigrationHttpTrigger);
    
    [Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post",Route = null)] HttpRequestData requestData)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            var request = await JsonSerializer.DeserializeAsync<MigrateApplicationReviewsHttpRequest>(requestData.Body);
            if (request?.VacancyReference > 0)
            {
                await applicationReviewMigrationStrategy.RunAsync(request.VacancyReference);
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