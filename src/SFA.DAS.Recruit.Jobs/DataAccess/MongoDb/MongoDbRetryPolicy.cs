using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;

[ExcludeFromCodeCoverage]
internal static class MongoDbRetryPolicy
{
    public static AsyncRetryPolicy GetRetryPolicy(ILogger logger)
    {
        var policyBuilderJittered = 
            Policy
                .Handle<MongoCommandException>()
                .Or<MongoWriteException>()
                .Or<MongoBulkWriteException>()
                .WaitAndRetryAsync(
                    Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(200), 10), 
                    (_, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning($"Throttled (429) {context.OperationKey}. Retrying {retryCount} in {timeSpan.TotalMilliseconds}ms...");
                    });
            
        return policyBuilderJittered;
    }
}