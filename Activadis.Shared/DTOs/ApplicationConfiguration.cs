namespace Activadis.Shared.DTOs
{
    public static class ApplicationConfiguration
    {
        public static long AllowedSizeInMB = SizeInMB(10);
        public static string BaseUrl = string.Empty;

        public static class RedirectLinks
        {
            public readonly static string ConfirmSignUp = "/confirm-signup";
            public readonly static string ConfirmSignOut = "/confirm-signout";
            public readonly static string ConfirmResignUp = "/confirm-resignup";
            public readonly static string SetupPassword = "/wachtwoord-instellen";
        }

        private static long SizeInMB(int size)
            => size * 1024 * 1024;
    }
}
