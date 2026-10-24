using Microsoft.AspNetCore.Components.Forms;

namespace Activadis.UI.Application.DTOs.Activity
{
	public class AddActivityPhotosRequest
	{
		public List<IBrowserFile> Photos { get; set; } = [];
	}
}