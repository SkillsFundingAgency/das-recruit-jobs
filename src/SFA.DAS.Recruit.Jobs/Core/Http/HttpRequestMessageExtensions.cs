namespace SFA.DAS.Recruit.Jobs.Core.Http;

public static class HttpRequestMessageExtensions
{
    public static void AddVersionHeader(this HttpRequestMessage request, string version)
    {
        request.Headers.TryAddWithoutValidation("X-Version", version);
    }
    
    public static void AddApimKeyHeader(this HttpRequestMessage request, string version)
    {
        request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", version);
    }
}