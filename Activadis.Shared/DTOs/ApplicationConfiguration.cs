namespace Activadis.Shared.DTOs
{
    public static class ApplicationConfiguration
    {
        public static long AllowedSizeInMB = SizeInMB(10);

        private static long SizeInMB(int size)
            => size * 1024 * 1024;
    }
}
