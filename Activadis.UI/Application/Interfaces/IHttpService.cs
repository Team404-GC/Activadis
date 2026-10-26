using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;
using Activadis.Shared.DTOs;

namespace Activadis.UI.Application.Interfaces
{
    public interface IHttpService
    {
        Task<ApiResponse<TResponse>> GetAsync<TResponse>(string url);
        Task<ApiResponse<TResponse>> QueryAsync<TResponse, TRequest>(string url, TRequest request);
        Task<ApiResponse<TResponse>> PostAsync<TResponse, TRequest>(string url, TRequest request);
        Task<ApiResponse<TResponse>> PostIncludeFilesAsync<TResponse, TRequest>(string url, TRequest request, Expression<Func<TRequest, List<IBrowserFile>>> property);
        Task<ApiResponse<TResponse>> PutAsync<TResponse, TRequest>(string url, TRequest request);
        Task<ApiResponse<TResponse>> PutIncludeFilesAsync<TResponse, TRequest>(string url, TRequest request, Expression<Func<TRequest, List<IBrowserFile>>> property);
        Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(string url);
    }
}