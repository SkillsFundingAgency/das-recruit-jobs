namespace SFA.DAS.Recruit.Jobs.Core.Http;

public static class HttpRequestMessageExtensions
{
    public static void AddVersionHeader(this HttpRequestMessage request, string version)
    {
        request.Headers.TryAddWithoutValidation("X-Version", version);
    }
}