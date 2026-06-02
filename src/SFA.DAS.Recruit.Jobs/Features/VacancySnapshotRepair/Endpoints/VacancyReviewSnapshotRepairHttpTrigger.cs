using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.Features.VacancySnapshotRepair.Endpoints;

[ExcludeFromCodeCoverage]
public class VacancyReviewSnapshotRepairHttpTrigger(ILogger<VacancyReviewSnapshotRepairHttpTrigger> logger,
    VacancyReviewSnapshotRepairStrategy vacancyReviewSnapshotRepairStrategy)
{
    private const string TriggerName = nameof(VacancyReviewSnapshotRepairHttpTrigger);

    [Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData _,
        FunctionContext context,
        CancellationToken token)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            await vacancyReviewSnapshotRepairStrategy.RunAsync(token);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[{TriggerName}] Unhandled Exception occured during repairing vacancyReview snapshot", TriggerName);
            throw;
        }
        finally
        {
            logger.LogInformation("[{TriggerName}] trigger completed", TriggerName);
        }
    }
}