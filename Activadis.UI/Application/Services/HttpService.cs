using Activadis.UI.Application.Interfaces;
using Activadis.Application.DTOs;
using System.Net.Http.Json;

namespace Activadis.UI.Application.Services
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient HttpClient;

        public HttpService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string url)
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.GetAsync(url);
                ApiResponse<TResponse>? result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
                return result ?? throw new ArgumentException();
            }
            catch
            {
                return Error<TResponse>();
            }
        }

        public async Task<ApiResponse<TResponse>> PostAsync<TResponse, TRequest>(string url, TRequest request)
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(url, request);
                ApiResponse<TResponse>? result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
                return result ?? throw new ArgumentException();
            }
            catch
            {
                return Error<TResponse>();
            }
        }

        public async Task<ApiResponse<TResponse>> PutAsync<TResponse, TRequest>(string url, TRequest request)
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(url, request);
                ApiResponse<TResponse>? result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
                return result ?? throw new ArgumentException();
            }
            catch
            {
                return Error<TResponse>();
            }
        }

        private static ApiResponse<TResponse> Error<TResponse>()
            => ApiResponse<TResponse>.Fail("Er is een onverwachte fout opgetreden.");
    }
}
