namespace Activadis.Application.Extensions
{
    public static class ConvertExtensions
    {
        public static bool TryFromHexString(string text, out byte[] bytes)
        {
            try
            {
                byte[] extracted = Convert.FromHexString(text);
                bytes = extracted;
                return true;
            }
            catch (FormatException)
            {
                bytes = [];
                return false;
            }
        }
    }
}
