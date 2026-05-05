using System.Net;

namespace SFA.DAS.Recruit.Jobs.Core.Http;

public record ApiResponse(HttpStatusCode StatusCode, string? ErrorContent = null)
{
    public bool Success => (int)StatusCode >= 200 && (int)StatusCode <= 299;
};

public record ApiResponse<T>(HttpStatusCode StatusCode, T? Payload, string? ErrorContent = null): ApiResponse(StatusCode, ErrorContent);