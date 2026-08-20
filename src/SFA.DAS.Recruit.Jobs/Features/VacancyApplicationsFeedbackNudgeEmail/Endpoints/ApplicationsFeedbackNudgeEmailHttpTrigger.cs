using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using SFA.DAS.Recruit.Jobs.Features.VacancyApplicationsFeedbackNudgeEmail.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyApplicationsFeedbackNudgeEmail.Endpoints;

public class ApplicationsFeedbackNudgeEmailHttpTrigger(IApplicationsFeedbackNudgeEmailHandler handler)
{
    private const string TriggerName = nameof(ApplicationsFeedbackNudgeEmailHttpTrigger);
    
    private class FeedbackHttpRequestData
    {
        [JsonPropertyName("date")]
        public required DateTime Date { get; init; }
    }

    [Function(TriggerName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData httpRequestData, CancellationToken cancellationToken)
    {
        var payload = await JsonSerializer.DeserializeAsync<FeedbackHttpRequestData>(httpRequestData.Body, cancellationToken: cancellationToken);
        var date = DateOnly.FromDateTime(payload?.Date ?? DateTime.UtcNow);
        await handler.RunAsync(date, cancellationToken);
    }
}