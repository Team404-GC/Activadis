namespace Activadis.Application.DTOs
{
    public static class ApplicationConfiguration
    {
        public static string BaseUrl = string.Empty;

        public static class RedirectLinks
        {
            public readonly static string ConfirmSignUp = "/confirm-signup";
            public readonly static string ConfirmSignOut = "/confirm-signout";
            public readonly static string ConfirmResignUp = "/confirm-resignup";
            public readonly static string SetupPassword = "/wachtwoord-instellen";
        }
    }
}
