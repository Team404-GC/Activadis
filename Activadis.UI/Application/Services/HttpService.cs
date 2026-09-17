using Microsoft.AspNetCore.Components.Forms;
using Activadis.UI.Application.Interfaces;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using Activadis.Shared.DTOs;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

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

        public async Task<ApiResponse<TResponse>> PostIncludeFileAsync<TResponse, TRequest>(string url, TRequest request, Expression<Func<TRequest, IBrowserFile?>> property)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                IBrowserFile? file = property.Compile()(request);

                if (property.Body is not MemberExpression expression || expression.Member is not PropertyInfo filePropertyInfo)
                    return Error<TResponse>();

                if (file is not null)
                {
                    Stream stream = file.OpenReadStream();
                    StreamContent streamContent = new StreamContent(stream);
                    if (file.ContentType.Trim().Length <= 0)
                        return ApiResponse<TResponse>.Fail("De foto heeft een ongeldig bestandstype.");

                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                    string parameterName = filePropertyInfo.Name ?? "file";
                    content.Add(streamContent, parameterName, file.Name);
                }

                foreach (PropertyInfo propertyInfo in typeof(TRequest).GetProperties())
                {
                    if (propertyInfo.Name == filePropertyInfo.Name)
                        continue;

                    object? value = propertyInfo.GetValue(request);
                    string actualValue = value switch
                    {
                        null => string.Empty,
                        DateTime dateTimeValue => dateTimeValue.ToString("o"),
                        string stringValue => stringValue,
                        _ when propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType.IsEnum || value is decimal => value.ToString() ?? string.Empty,
                        _ => JsonSerializer.Serialize(value)
                    };

                    content.Add(new StringContent(actualValue), propertyInfo.Name);
                }

                using HttpResponseMessage response = await HttpClient.PostAsync(url, content);
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

        public async Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(string url)
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.DeleteAsync(url);
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
