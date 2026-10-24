using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Activadis.UI.Application.DTOs;
using Activadis.UI.Application.Services;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Shared.DTOs;
using System.Reflection;

namespace Activadis.UI.Application.Extensions
{
    public static class ComponentRequestExtensions
    {
        public static void InstantUpdateComment<TRequest>(this ComponentRequest<TRequest> request, ChangeEventArgs args) where TRequest : HasCommentBase
        {
            request.Request.Comment = args.Value?.ToString();
            FieldIdentifier field = new FieldIdentifier(request.Request, nameof(request.Request.Comment));
            request.Context.NotifyFieldChanged(field);
        }
    

        public static int GetStatusCode<T>(this ApiResponse<T> response)
            => response is HttpApiResponse<T> httpResponse ? httpResponse.StatusCode : 0;

        public static bool IsInputError<T>(this ApiResponse<T> response)
            => !response.Succeeded && response.GetStatusCode() == 400;

        public static string? HandleError<T>(this ApiResponse<T> response)
        {
            if (!response.IsInputError())
            {
                Notifications.AddNotification(response.Message, true);
                return null;
            }

            return response.Errors.Any()
                ? string.Join(" ", response.Errors.SelectMany(x => x.Errors))
                : response.Message ?? "De invoer is ongeldig.";
        }

        public static void ShowErrors<TRequest, TResponse>(this ComponentRequest<TRequest> request, ApiResponse<TResponse> response)
        {
            request.MessageStore.Clear();
            request.FormError = null;

            if (!response.IsInputError())
            {
                Notifications.AddNotification(response.Message, true);
                request.Context.NotifyValidationStateChanged();
                return;
            }

            List<string> formErrors = [];
            foreach (ApiErrors errors in response.Errors)
            {
                PropertyInfo? property = typeof(TRequest).GetProperty(errors.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property is null)
                {
                    formErrors.AddRange(errors.Errors);
                    continue;
                }

                FieldIdentifier field = request.Context.Field(property.Name);
                foreach (string error in errors.Errors)
                    request.MessageStore.Add(field, error);
            }

            if (!response.Errors.Any())
                formErrors.Add(response.Message ?? "De invoer is ongeldig.");

            request.FormError = formErrors.Any() ? string.Join(" ", formErrors) : null;
            request.Context.NotifyValidationStateChanged();
        }

        public static void ShowFormError<TRequest>(this ComponentRequest<TRequest> request, string message)
        {
            request.FormError = message;
            request.Context.NotifyValidationStateChanged();
        }
    }
}
