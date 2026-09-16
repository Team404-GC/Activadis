namespace Activadis.Application.Extensions
{
    public static class ImageExtensions
    {
        private const string UnknownContentType = "application/octet-stream";

        private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47];
        private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
        private static readonly byte[] RiffSignature = [0x52, 0x49, 0x46, 0x46];
        private static readonly byte[] WebpSignature = [0x57, 0x45, 0x42, 0x50];

        /// <summary>
        /// The content type is not stored on the activity, so it is determined from the
        /// file signature of the stored bytes.
        /// </summary>
        public static string ToContentType(this byte[] image)
        {
            if (image.HasSignature(PngSignature, 0))
                return "image/png";

            if (image.HasSignature(JpegSignature, 0))
                return "image/jpeg";

            if (image.HasSignature(RiffSignature, 0) && image.HasSignature(WebpSignature, 8))
                return "image/webp";

            return UnknownContentType;
        }

        private static bool HasSignature(this byte[] image, byte[] signature, int offset)
            => image.Length >= offset + signature.Length
                && image.Skip(offset).Take(signature.Length).SequenceEqual(signature);
    }
}
