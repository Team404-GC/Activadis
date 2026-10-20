using Microsoft.AspNetCore.Http;

namespace Activadis.Application.DTOs.Activity
{
    public class CreateActivityRequest : ActivityRequest
    {
        public override required IFormFile? Image { get; set; }

        public bool ExternalAllowed { get; set; }
        public bool PlusOneAllowed { get; set; }

        public List<Guid> CategoryIds { get; set; } = [];
    }
}
