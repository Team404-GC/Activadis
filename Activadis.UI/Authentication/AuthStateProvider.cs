using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Activadis.UI.Authentication
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private const string TokenKey = "authToken";

        private readonly SessionStorageService _sessionStorageService;

        private readonly AuthenticationState _anonymous =
            new(new ClaimsPrincipal(new ClaimsIdentity()));

        private AuthenticationState? _currentState;

        public AuthStateProvider(SessionStorageService sessionStorageService)
        {
            _sessionStorageService = sessionStorageService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_currentState != null)
                return _currentState;

            try
            {
                string? token = await _sessionStorageService.GetItemAsync(TokenKey);

                if (string.IsNullOrWhiteSpace(token))
                {
                    _currentState = _anonymous;
                    return _currentState;
                }

                List<Claim> claims = ParseJWT.GetClaims(token);

                if (IsExpired(claims))
                {
                    await _sessionStorageService.RemoveItemAsync(TokenKey);

                    _currentState = _anonymous;
                    return _currentState;
                }

                _currentState = new AuthenticationState(BuildUser(claims));
                return _currentState;
            }
            catch
            {
                _currentState = _anonymous;
                return _currentState;
            }
        }

        private static bool IsExpired(List<Claim> claims)
        {
            Claim? exp = claims.FirstOrDefault(c => c.Type == "exp");

            if (exp == null || !long.TryParse(exp.Value, out long seconds))
                return true;

            return DateTimeOffset.FromUnixTimeSeconds(seconds) <= DateTimeOffset.UtcNow;
        }

        private static ClaimsPrincipal BuildUser(List<Claim> claims)
        {
            string roleClaimType =
                claims.Any(c => c.Type == ClaimTypes.Role)
                    ? ClaimTypes.Role
                    : "role";

            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    claims,
                    "jwt",
                    ClaimTypes.Name,
                    roleClaimType));
        }

        public async Task MarkUserAsAuthenticatedAsync(string token)
        {
            await _sessionStorageService.SetItemAsync(TokenKey, token);

            List<Claim> claims = ParseJWT.GetClaims(token);

            _currentState = new AuthenticationState(BuildUser(claims));

            NotifyAuthenticationStateChanged(
                Task.FromResult(_currentState));
        }

        public async Task MarkUserAsLoggedOutAsync()
        {
            await _sessionStorageService.RemoveItemAsync(TokenKey);

            _currentState = _anonymous;

            NotifyAuthenticationStateChanged(
                Task.FromResult(_anonymous));
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _sessionStorageService.GetItemAsync(TokenKey);
        }
    }
}