using Microsoft.JSInterop;

namespace Activadis.UI.Authentication
{
	public class SessionStorageService
	{
		private readonly IJSRuntime JavaScript;

		public SessionStorageService(IJSRuntime javaScript)
		{
			JavaScript = javaScript;
		}

		public async Task SetItemAsync(string key, string value)
			=> await JavaScript.InvokeVoidAsync("sessionStorage.setItem", key, value);

		public async Task<string?> GetItemAsync(string key)
			=> await JavaScript.InvokeAsync<string?>("sessionStorage.getItem", key);

		public async Task RemoveItemAsync(string key)
			=> await JavaScript.InvokeVoidAsync("sessionStorage.removeItem", key);
	}
}
