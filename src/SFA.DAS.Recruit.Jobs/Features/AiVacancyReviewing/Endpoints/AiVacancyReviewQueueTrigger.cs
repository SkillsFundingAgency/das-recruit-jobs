using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Core.Infrastructure;
using SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Clients;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SFA.DAS.Recruit.Jobs.Features.AiVacancyReviewing.Endpoints;

[ExcludeFromCodeCoverage]
public class AiVacancyReviewQueueTrigger(IRecruitAiOuterClient recruitAiOuterClient, JsonSerializerOptions jsonSerializerOptions)
{
    [Function(nameof(AiVacancyReviewQueueTrigger))]
    public async Task Run([QueueTrigger(StorageConstants.QueueNames.AiVacancyReviewRequests)] QueueMessage message, CancellationToken cancellationToken)
    {
        var queueItem = JsonSerializer.Deserialize<QueueItem<AiVacancyReviewMessage>>(message.Body, jsonSerializerOptions);
        if (queueItem is null)
        {
            throw new JsonSerializationException($"Failed to deserialize ai review message: '{message.Body}'");
        }
        
        var response = await recruitAiOuterClient.ReviewVacancyAsync(queueItem.Payload.VacancyId, queueItem.Payload.VacancyReviewId, cancellationToken);
        if (!response.Success)
        {
            // make sure the message is retried
            throw new ApiException("Failed to perform vacancy ai review", response);
        }
    }
}