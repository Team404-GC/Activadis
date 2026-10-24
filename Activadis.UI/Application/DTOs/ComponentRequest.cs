using Microsoft.AspNetCore.Components.Forms;

namespace Activadis.UI.Application.DTOs
{
    public class ComponentRequest<TRequest> : IDisposable
    {
        public TRequest Request { get; set; }
        public bool ShouldOpenComponent { get; set; }
        public bool IsLoading { get; set; }
        public EditContext Context { get; set; }
        public ValidationMessageStore MessageStore { get; set; }
        public string? FormError { get; set; }

        public ComponentRequest(TRequest request)
        {
            Request = request;
            Context = new EditContext(Request!);
            MessageStore = new ValidationMessageStore(Context);

            Context.OnFieldChanged += OnFieldChanged;
        }

        public void Dispose()
            => Context.OnFieldChanged -= OnFieldChanged;

        private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
        {
            MessageStore.Clear(args.FieldIdentifier);
            FormError = null;
            Context.MarkAsUnmodified(args.FieldIdentifier);
            Context.NotifyValidationStateChanged();
        }
    }
}
