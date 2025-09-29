using System.Net;

namespace SFA.DAS.Recruit.Jobs.Core.Http;

public record ApiResponse(bool Ok, HttpStatusCode StatusCode, string? ErrorContent = null);

public record ApiResponse<T>(bool Ok, HttpStatusCode StatusCode, T? Payload, string? ErrorContent = null): ApiResponse(Ok, StatusCode, ErrorContent);