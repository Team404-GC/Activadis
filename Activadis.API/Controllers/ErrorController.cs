using Activadis.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Activadis.API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult HandleError()
        {
            return StatusCode(500, ApiResponse<string>.Fail("Er is een onverwachte fout opgetreden."));
        }
    }
}
