using System.Net;

namespace SFA.DAS.Recruit.Jobs.Core.Http;

public record ApiResponse(bool Success, HttpStatusCode StatusCode, string? ErrorContent = null);

public record ApiResponse<T>(bool Success, HttpStatusCode StatusCode, T? Payload, string? ErrorContent = null): ApiResponse(Success, StatusCode, ErrorContent);