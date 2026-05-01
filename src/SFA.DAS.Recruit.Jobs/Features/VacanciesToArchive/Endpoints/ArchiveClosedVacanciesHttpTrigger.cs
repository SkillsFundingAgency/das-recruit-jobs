using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.VacanciesToArchive.Endpoints;

[ExcludeFromCodeCoverage]
public class ArchiveClosedVacanciesHttpTrigger(ILogger<ArchiveClosedVacanciesHttpTrigger> logger,
    IOptions<Core.Configuration.Features> features,
    IArchiveClosedVacanciesHandler handler)
{
    private const string TriggerName = nameof(ArchiveClosedVacanciesHttpTrigger);
    private readonly Core.Configuration.Features _features = features.Value;

    [Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData _,
        FunctionContext context,
        CancellationToken token)
    {
        logger.LogInformation("[{TriggerName}] Trigger fired", TriggerName);
        try
        {
            // Check if the feature flag to archive vacancies without outcome is enabled. If it is, skip the archiving process.
            // This is to prevent archiving closed vacancies without outcome until the feature flag is disabled, which will be done after the migration of vacancies to archive is complete, we are ready to switch over to the new process.
            if (!_features.ArchiveVacanciesWithoutOutCome)
            {
                logger.LogInformation("[{TriggerName}] Feature flag {FeatureFlag} is enabled. Skipping archiving closed vacancies without outcome.",
                    TriggerName, nameof(Core.Configuration.Features.ArchiveVacanciesWithoutOutCome));
                return;
            }

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