using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Activadis.UI.Authentication
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private const string TokenKey = "AUTH_TOKEN";
        private readonly AuthenticationState Anonymous = new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity()
            )
        );

        private readonly SessionStorageService SessionStorageService;

        public AuthStateProvider(SessionStorageService sessionStorageService)
        {
            SessionStorageService = sessionStorageService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? token = await SessionStorageService.GetItemAsync(TokenKey);
            if (string.IsNullOrWhiteSpace(token))
                return Anonymous;

            List<Claim> claims = ParseJWT.GetClaims(token);
            if (IsExpired(claims))
            {
                await SessionStorageService.RemoveItemAsync(TokenKey);
                return Anonymous;
            }

            return GetAuthenticationState(claims);
        }

        public async Task MarkUserAsAuthenticatedAsync(string token)
        {
            await SessionStorageService.SetItemAsync(TokenKey, token);
            List<Claim> claims = ParseJWT.GetClaims(token);

            NotifyAuthenticationStateChanged(
                Task.FromResult(
                    GetAuthenticationState(claims)
                )
            );
        }

        public async Task MarkUserAsLoggedOutAsync()
        {
            await SessionStorageService.RemoveItemAsync(TokenKey);

            NotifyAuthenticationStateChanged(
                Task.FromResult(Anonymous)
            );
        }

        public async Task<string?> GetTokenAsync()
            => await SessionStorageService.GetItemAsync(TokenKey);

        private static AuthenticationState GetAuthenticationState(List<Claim> claims)
        {
            return new AuthenticationState(
                new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "JWT")
                )
            );
        }

        private static bool IsExpired(List<Claim> claims)
        {
            Claim? expires = claims.FirstOrDefault(x => x.Type == "exp");

            if (expires is null || !long.TryParse(expires.Value, out long seconds))
                return true;

            return DateTimeOffset.FromUnixTimeSeconds(seconds) <= DateTimeOffset.UtcNow;
        }
    }
}