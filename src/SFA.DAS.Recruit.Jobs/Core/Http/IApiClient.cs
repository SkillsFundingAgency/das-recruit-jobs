namespace SFA.DAS.Recruit.Jobs.Core.Http;

public interface IApiClient
{
    Task<ApiResponse<TResponse>> GetAsync<TResponse>(IGetRequest request, CancellationToken cancellationToken = default);
    async Task<ApiResponse> GetAsync(IGetRequest request, CancellationToken cancellationToken = default) => await GetAsync<NoResponse>(request, cancellationToken);
    Task<ApiResponse<TResponse>> PostAsync<TResponse>(IPostRequest request, CancellationToken cancellationToken = default);
    async Task<ApiResponse> PostAsync(IPostRequest request, CancellationToken cancellationToken = default) => await PostAsync<NoResponse>(request, cancellationToken);
    Task<ApiResponse<TResponse>> PutAsync<TResponse>(IPutRequest request, CancellationToken cancellationToken = default);
    async Task<ApiResponse> PutAsync(IPutRequest request, CancellationToken cancellationToken = default) => await PutAsync<NoResponse>(request, cancellationToken);
    Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(IDeleteRequest request, CancellationToken cancellationToken = default);
    async Task<ApiResponse> DeleteAsync(IDeleteRequest request, CancellationToken cancellationToken = default) => await DeleteAsync<NoResponse>(request, cancellationToken);
    Task<ApiResponse<TResponse>> PatchAsync<TResponse>(IPatchRequest request, CancellationToken cancellationToken = default);
    async Task<ApiResponse> PatchAsync(IPatchRequest request, CancellationToken cancellationToken = default) => await PatchAsync<NoResponse>(request, cancellationToken);
}