using Activadis.Application.DTOs;
using Activadis.UI.Application;
using System.Net.Http.Json;

namespace Activadis.UI.Application.Services
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;

        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string url)
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(url);
                ApiResponse<TResponse>? result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
                return result ?? throw new ArgumentException();
            }
            catch
            {
                return ApiResponse<TResponse>.Fail("Er is een onverwachte fout opgetreden.");
            }
        }

        public async Task<ApiResponse<TResponse>> PostAsync<TResponse, TRequest>(string url, TRequest request)
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, request);
                ApiResponse<TResponse>? result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
                return result ?? throw new ArgumentException();
            }
            catch
            {
                return ApiResponse<TResponse>.Fail("Er is een onverwachte fout opgetreden.");
            }
        }

        public async Task<ApiResponse<TResponse>> PutAsync<TResponse, TRequest>(string url, TRequest request)
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.PutAsJsonAsync(url, request);
                ApiResponse<TResponse>? result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
                return result ?? throw new ArgumentException();
            }
            catch
            {
                return ApiResponse<TResponse>.Fail("Er is een onverwachte fout opgetreden.");
            }
        }
    }
}
