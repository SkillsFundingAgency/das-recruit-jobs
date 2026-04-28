using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Endpoints;

[ExcludeFromCodeCoverage]
public class ArchiveClosedVacanciesHttpTrigger(ILogger<ArchiveClosedVacanciesHttpTrigger> logger,
    IArchiveClosedVacanciesHandler handler)
{
    private const string TriggerName = nameof(ArchiveClosedVacanciesHttpTrigger);

    [Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData _,
        FunctionContext context,
        CancellationToken token)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await handler.RunAsync(token);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during archiving vacancies", TriggerName);
            throw;
        }
        finally
        {
            logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
        }
    }
}