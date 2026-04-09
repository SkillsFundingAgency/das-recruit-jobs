using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Core.Configuration;

namespace SFA.DAS.Recruit.Jobs.Core.Http;

public abstract class ApiClientBase<TClientConfig>: IApiClient
    where TClientConfig : IClientConfig
{
    private readonly HttpClient _httpClient;
    private readonly TClientConfig _config;
    private readonly JsonSerializerOptions _jsonSerializationOptions;

    protected ApiClientBase(HttpClient httpClient, TClientConfig config, JsonSerializerOptions jsonSerializationOptions)
    {
        _httpClient = httpClient;
        _config = config;
        _jsonSerializationOptions = jsonSerializationOptions;
        httpClient.BaseAddress = new Uri(config.BaseUrl!);
    }

    protected virtual HttpRequestMessage CreateRequest(HttpMethod method, string url, string apiVersion)
    {
        var request = new HttpRequestMessage(method, url);
        request.AddApimKeyHeader(_config.Key!);
        request.AddVersionHeader(apiVersion);
        return request;
    }

    protected virtual async Task<ApiResponse<TResponse>> ProcessResponse<TResponse>(HttpResponseMessage response)
    {
        string? content;
        if (!response.IsSuccessStatusCode)
        {
            content = await response.Content.ReadAsStringAsync();
            return new ApiResponse<TResponse>(response.StatusCode, default, content);
        }

        if (typeof(TResponse) == typeof(NoResponse) || response.StatusCode is HttpStatusCode.NoContent)
        {
            return new ApiResponse<TResponse>(response.StatusCode, default);
        }

        content = await response.Content.ReadAsStringAsync();
        var payload = JsonSerializer.Deserialize<TResponse>(content, _jsonSerializationOptions);
        return new ApiResponse<TResponse>(response.StatusCode, payload);
    }
    
    protected virtual async Task<ApiResponse<TResponse>> SendAsync<TResponse>(HttpMethod method, string url, object? data, string version, CancellationToken cancellationToken)
    {
        var apiRequest = CreateRequest(method, url, version);
        if (data is not null)
        {
            apiRequest.Content = JsonContent.Create(data, null, _jsonSerializationOptions);
        }
        
        var response = await _httpClient.SendAsync(apiRequest, cancellationToken);
        return await ProcessResponse<TResponse>(response);
    }

    public virtual async Task<ApiResponse<T>> GetAsync<T>(IGetRequest request, CancellationToken cancellationToken = default)
        => await SendAsync<T>(HttpMethod.Get, request.Url, null, request.Version, cancellationToken);

    public virtual async Task<ApiResponse<TResponse>> PostAsync<TResponse>(IPostRequest request, CancellationToken cancellationToken = default)
        => await SendAsync<TResponse>(HttpMethod.Post, request.Url, request.Data, request.Version, cancellationToken);
    
    public virtual async Task<ApiResponse<TResponse>> PutAsync<TResponse>(IPutRequest request, CancellationToken cancellationToken = default)
        => await SendAsync<TResponse>(HttpMethod.Put, request.Url, request.Data, request.Version, cancellationToken);
    
    public virtual async Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(IDeleteRequest request, CancellationToken cancellationToken = default)
        => await SendAsync<TResponse>(HttpMethod.Delete, request.Url, null, request.Version, cancellationToken);
    
    public virtual async Task<ApiResponse<TResponse>> PatchAsync<TResponse>(IPatchRequest request, CancellationToken cancellationToken = default)
        => await SendAsync<TResponse>(HttpMethod.Patch, request.Url, request.Data, request.Version, cancellationToken);
}