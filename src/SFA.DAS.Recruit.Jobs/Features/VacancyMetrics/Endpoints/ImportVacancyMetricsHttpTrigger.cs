using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Features.VacancyMetrics.Handlers;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyMetrics.Endpoints;

[ExcludeFromCodeCoverage]
public class ImportVacancyMetricsHttpTrigger(ILogger<ImportVacancyMetricsHttpTrigger> logger,
    IImportVacancyMetricsHandler handler)
{
    private const string TriggerName = nameof(ImportVacancyMetricsHttpTrigger);

    [Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData _, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await handler.RunAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during importing vacancy analytics data", TriggerName);
            throw;
        }
        finally
        {
            logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
        }
    }
}