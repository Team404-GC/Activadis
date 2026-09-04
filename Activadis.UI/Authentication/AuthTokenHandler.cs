using System.Net.Http.Headers;

namespace Activadis.UI.Authentication
{
	public class AuthTokenHandler : DelegatingHandler
	{
		private readonly AuthStateProvider AuthStateProvider;

		public AuthTokenHandler(AuthStateProvider authStateProvider)
		{
			AuthStateProvider = authStateProvider;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			string? token = await AuthStateProvider.GetTokenAsync();

			if (!string.IsNullOrWhiteSpace(token))
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			return await base.SendAsync(request, cancellationToken);
		}
	}
}
