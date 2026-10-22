namespace Activadis.UI.Application.DTOs
{
    public class ComponentRequest<TRequest>
    {
        public TRequest Request { get; set; }
        public bool ShouldOpenComponent { get; set; }
        public bool IsLoading { get; set; }

        public ComponentRequest(TRequest request)
        {
            Request = request;
        }
    }
}
