using System.Net.Http.Json;
using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;

namespace SFA.DAS.Recruit.Jobs.Core.Http;

public abstract class ClientBase<TClientConfig> where TClientConfig : IClientConfig
{
    private readonly HttpClient _httpClient;
    private readonly TClientConfig _config;
    private readonly JsonSerializerOptions _jsonSerializationOptions;

    protected ClientBase(HttpClient httpClient, TClientConfig config, JsonSerializerOptions jsonSerializationOptions)
    {
        _httpClient = httpClient;
        _config = config;
        _jsonSerializationOptions = jsonSerializationOptions;
        httpClient.BaseAddress = new Uri(config.BaseUrl!);
    }
    
    private const string ApiVersionOne = "1";

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, string apiVersion = ApiVersionOne)
    {
        var request = new HttpRequestMessage(method, new Uri(_httpClient.BaseAddress!, url));
        request.AddApimKeyHeader(_config.Key!);
        request.AddVersionHeader(apiVersion);
        return request;
    }

    private async Task<ApiResponse<T>> ProcessResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<T>(response.StatusCode, default, content);
        }

        if (typeof(T) == typeof(NoResponse))
        {
            return new ApiResponse<T>(response.StatusCode, default);
        }
        
        var payload = JsonSerializer.Deserialize<T>(content, _jsonSerializationOptions);
        return new ApiResponse<T>(response.StatusCode, payload);
    }
    
    protected async Task<ApiResponse<T>> GetAsync<T>(
        string url,
        string apiVersion = ApiVersionOne,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, url, apiVersion);
        var response = await _httpClient.SendAsync(request, cancellationToken);
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
            request.Content = JsonContent.Create(payload, null, _jsonSerializationOptions);
        }
        
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ProcessResponse<T>(response);
    }

    protected async Task<ApiResponse<T>> PutAsync<T>(
        string url,
        object? payload = null,
        string apiVersion = ApiVersionOne,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Put, url, apiVersion);
        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload, null, _jsonSerializationOptions);
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ProcessResponse<T>(response);
    }
}