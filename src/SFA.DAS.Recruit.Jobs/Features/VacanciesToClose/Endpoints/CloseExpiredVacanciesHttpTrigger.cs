using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Features.VacanciesToClose.Handlers;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToClose.Endpoints;

[ExcludeFromCodeCoverage]
public class CloseExpiredVacanciesHttpTrigger(
    ILogger<CloseExpiredVacanciesHttpTrigger> logger,
    ICloseExpiredVacanciesHandler handler)
{
    private const string TriggerName = nameof(CloseExpiredVacanciesHttpTrigger);

    [Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData _, CancellationToken token)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await handler.RunAsync(token);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during marking vacancies to close", TriggerName);
            throw;
        }
        finally
        {
            logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
        }
    }
}