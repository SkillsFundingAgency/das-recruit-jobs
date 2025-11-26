namespace SFA.DAS.Recruit.Jobs.Core.Http;

public class ApiException(string message, ApiResponse? apiResponse): Exception(FormatMessage(message, apiResponse))
{
    private static string FormatMessage(string message, ApiResponse? apiResponse)
    {
        var statusCode = apiResponse?.StatusCode.ToString() ?? "unknown";
        var errorContent = apiResponse?.ErrorContent ?? "unknown";
        return $"{message}. The remote API returned '{statusCode}' with detail '{errorContent}'";
    }
}