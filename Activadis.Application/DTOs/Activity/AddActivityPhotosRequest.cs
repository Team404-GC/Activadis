using Microsoft.AspNetCore.Http;

namespace Activadis.Application.DTOs.Activity
{
	public class AddActivityPhotosRequest
	{
		public List<IFormFile> Photos { get; set; } = [];
	}
}
