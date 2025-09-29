using System.Net.Http.Json;
using System.Text.Json;

namespace SFA.DAS.Recruit.Jobs.Core.Http;

public abstract class ClientBase(HttpClient httpClient, JsonSerializerOptions jsonSerializationOptions)
{
    private const string ApiVersionOne = "1.0";
    
    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string apiVersion = ApiVersionOne)
    {
        var request = new HttpRequestMessage(method, url);
        request.AddVersionHeader(apiVersion);
        return request;
    }

    private async Task<ApiResponse<T>> ProcessResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<T>(response.IsSuccessStatusCode, response.StatusCode, default, content);
        }

        if (typeof(T) == typeof(NoResponse))
        {
            return new ApiResponse<T>(response.IsSuccessStatusCode, response.StatusCode, default);
        }
        
        var payload = JsonSerializer.Deserialize<T>(content, jsonSerializationOptions);
        return new ApiResponse<T>(response.IsSuccessStatusCode, response.StatusCode, payload);
    }
    
    protected async Task<ApiResponse<T>> GetAsync<T>(
        string url,
        string apiVersion = ApiVersionOne,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, url, apiVersion);
        var response = await httpClient.SendAsync(request, cancellationToken);
        return await ProcessResponse<T>(response);
    }
    
    protected async Task<ApiResponse<T>> PostAsync<T>(
        string url, 
        object? payload = null,
        string apiVersion = ApiVersionOne,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Post, url, apiVersion);
        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload, null, jsonSerializationOptions);
        }
        
        var response = await httpClient.SendAsync(request, cancellationToken);
        return await ProcessResponse<T>(response);
    }
}