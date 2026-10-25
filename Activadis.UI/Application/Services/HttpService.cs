using Microsoft.AspNetCore.Components.Forms;
using Activadis.UI.Application.DTOs;
using Activadis.UI.Application.Interfaces;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using Activadis.Shared.DTOs;
using System.Net.Http.Json;
using System.Collections;
using System.Reflection;
using System.Text.Json;

namespace Activadis.UI.Application.Services
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient HttpClient;
        private readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        public HttpService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string url)
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.GetAsync(url);
                return await ReadAsync<TResponse>(response);
            }
            catch
            {
                return Error<TResponse>();
            }
        }
        public async Task<ApiResponse<TResponse>> QueryAsync<TResponse, TRequest>(string url, TRequest request)
        {
            try
            {
                using HttpRequestMessage message = new HttpRequestMessage(new HttpMethod("QUERY"), url)
                {
                    Content = JsonContent.Create(request)
                };

                using HttpResponseMessage response = await HttpClient.SendAsync(message);
                return await ReadAsync<TResponse>(response);
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
                return await ReadAsync<TResponse>(response);
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
                using MultipartFormDataContent content = BuildContent<TResponse, TRequest>(request, property);
                using HttpResponseMessage response = await HttpClient.PostAsync(url, content);
                return await ReadAsync<TResponse>(response);
            }
            catch (ArgumentException exception)
            {
                return InputError<TResponse>(exception.Message);
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
                return await ReadAsync<TResponse>(response);
            }
            catch
            {
                return Error<TResponse>();
            }
        }

        public async Task<ApiResponse<TResponse>> PutIncludeFileAsync<TResponse, TRequest>(string url, TRequest request, Expression<Func<TRequest, IBrowserFile?>> property)
        {
            try
            {
                using MultipartFormDataContent content = BuildContent<TResponse, TRequest>(request, property);
                using HttpResponseMessage response = await HttpClient.PutAsync(url, content);
                return await ReadAsync<TResponse>(response);
            }
            catch (ArgumentException exception)
            {
                return InputError<TResponse>(exception.Message);
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
                return await ReadAsync<TResponse>(response);
            }
            catch
            {
                return Error<TResponse>();
            }
        }

        private async Task<ApiResponse<TResponse>> ReadAsync<TResponse>(HttpResponseMessage response)
        {
            HttpApiResponse<TResponse> result = await response.Content.ReadFromJsonAsync<HttpApiResponse<TResponse>>(JsonOptions)
                ?? throw new InvalidOperationException();

            result.StatusCode = (int)response.StatusCode;
            return result;
        }

        private static ApiResponse<TResponse> Error<TResponse>()
            => ApiResponse<TResponse>.Fail("Er is een onverwachte fout opgetreden.");

        private static ApiResponse<TResponse> InputError<TResponse>(string message)
        {
            return new HttpApiResponse<TResponse>()
            {
                Succeeded = false,
                Message = message,
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        private static MultipartFormDataContent BuildContent<TResponse, TRequest>(TRequest request, Expression<Func<TRequest, IBrowserFile?>> property)
        {
            MultipartFormDataContent content = new MultipartFormDataContent();
            IBrowserFile? file = property.Compile()(request);

            if (property.Body is not MemberExpression expression || expression.Member is not PropertyInfo filePropertyInfo)
                throw new InvalidOperationException();

            if (file is not null)
            {
                Stream stream = file.OpenReadStream(ApplicationConfiguration.AllowedSizeInMB);
                StreamContent streamContent = new StreamContent(stream);
                if (file.ContentType.Trim().Length <= 0)
                    throw new ArgumentException("De foto heeft een ongeldig bestandstype.");

                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                string parameterName = filePropertyInfo.Name ?? "file";
                content.Add(streamContent, parameterName, file.Name);
            }

            foreach (PropertyInfo propertyInfo in typeof(TRequest).GetProperties())
            {
                if (propertyInfo.Name == filePropertyInfo.Name)
                    continue;

                object? value = propertyInfo.GetValue(request);
                if (value is IEnumerable values and not string)
                {
                    foreach (object? item in values)
                        content.Add(new StringContent(item?.ToString() ?? string.Empty), propertyInfo.Name);

                    continue;
                }

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

            return content;
        }
    }

    public class HttpApiResponse<T> : ApiResponse<T>
    {
        public int StatusCode { get; set; }
    }
}
