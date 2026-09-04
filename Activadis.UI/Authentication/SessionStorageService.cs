using Microsoft.JSInterop;

namespace Activadis.UI.Authentication
{
	public class SessionStorageService
	{
		private readonly IJSRuntime _javaScript;

		public SessionStorageService(IJSRuntime javaScript)
		{
			_javaScript = javaScript;
		}

		public async Task SetItemAsync(string key, string value)
		{
			await _javaScript.InvokeVoidAsync("sessionStorage.setItem", key, value);
		}

		public async Task<string?> GetItemAsync(string key)
		{
			return await _javaScript.InvokeAsync<string?>("sessionStorage.getItem", key);
		}

		public async Task RemoveItemAsync(string key)
		{
			await _javaScript.InvokeVoidAsync("sessionStorage.removeItem", key);

		}
	}
}
