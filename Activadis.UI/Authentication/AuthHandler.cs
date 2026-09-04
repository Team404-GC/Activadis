using System.Net.Http.Headers;

namespace Activadis.UI.Authentication
{
	public class AuthHandler : DelegatingHandler
	{
		private readonly AuthStateProvider _authStateProvider;

		public AuthHandler(AuthStateProvider authStateProvider)
		{
			_authStateProvider = authStateProvider;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			string? token = await _authStateProvider.GetTokenAsync();

			if (!string.IsNullOrWhiteSpace(token))
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			return await base.SendAsync(request, cancellationToken);
		}
	}
}
