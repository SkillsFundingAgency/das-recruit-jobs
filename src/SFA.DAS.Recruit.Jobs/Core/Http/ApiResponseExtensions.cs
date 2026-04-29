using System.Net;

namespace SFA.DAS.Recruit.Jobs.Core.Http;

public static class ApiResponseExtensions
{
    public static bool NotFound<T>(this ApiResponse<T> response)
    {
        return response.StatusCode == HttpStatusCode.NotFound;
    }

    public static void ThrowIfErrored<T>(this ApiResponse<T> response, string message = "Api call failed") => ((ApiResponse)response).ThrowIfErrored(message);
    
    public static void ThrowIfErrored(this ApiResponse response, string message = "Api call failed")
    {
        if (!response.Success)
        {
            throw new ApiException(message, response);
        }
    }
}