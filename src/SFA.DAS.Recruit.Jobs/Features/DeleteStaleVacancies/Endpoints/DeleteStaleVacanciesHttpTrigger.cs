using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Features.DeleteStaleVacancies.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.DeleteStaleVacancies.Endpoints;

[ExcludeFromCodeCoverage]
public class DeleteStaleVacanciesHttpTrigger(ILogger<DeleteStaleVacanciesHttpTrigger> logger,
    IDeleteStaleVacanciesHandler handler)
{
    private const string TriggerName = nameof(DeleteStaleVacanciesHttpTrigger);

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