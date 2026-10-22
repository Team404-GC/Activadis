using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Activadis.UI.Application.DTOs;
using Activadis.Shared.DTOs.SignUp;

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
    }
}
