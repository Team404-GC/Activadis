using Microsoft.AspNetCore.Http;
using Activadis.Shared.DTOs;
using System.Text;

namespace Activadis.Application.Validations.Activity
{
	public static class ActivityPhotoValidation
	{
		private static readonly List<string> ValidImageTypes = ["image/png", "image/jpeg", "image/webp"];

		public static void ValidatePhotos(this IEnumerable<IFormFile> photos, int photoCount)
		{
			if (photoCount + photos.Count() > ApplicationConfiguration.MaxPhotos)
				throw new ArgumentException($"Een activiteit kan maximaal {ApplicationConfiguration.MaxPhotos} foto's hebben.");

			foreach (IFormFile photo in photos)
			{
				if (photo.Length <= 0)
					throw new ArgumentException($"De foto {photo.FileName} heeft een ongeldige bestandsgrootte.");

				if (!ValidImageTypes.Contains(photo.ContentType, StringComparer.OrdinalIgnoreCase))
					throw new ArgumentException($"De foto {photo.FileName} heeft een ongeldig bestandstype.");
			}
		}

		public static void ValidateImage(this byte[] image, string fileName)
		{
			bool isPng = image.Length > 8 && image[0] == 0x89 && image[1] == 0x50 && image[2] == 0x4E && image[3] == 0x47;
			bool isJpeg = image.Length > 3 && image[0] == 0xFF && image[1] == 0xD8 && image[2] == 0xFF;
			bool isWebp = image.Length > 12 && Encoding.ASCII.GetString(image, 0, 4) == "RIFF" && Encoding.ASCII.GetString(image, 8, 4) == "WEBP";

			if (!isPng && !isJpeg && !isWebp)
				throw new ArgumentException($"De foto {fileName} is geen geldige afbeelding.");
		}

		public static void ValidatePhotoOrder(this List<Guid> photoIds, IEnumerable<Guid> existingPhotoIds)
		{
			if (photoIds.Count != existingPhotoIds.Count() || photoIds.Distinct().Count() != photoIds.Count || existingPhotoIds.Any(x => !photoIds.Contains(x)))
				throw new ArgumentException("De volgorde van de foto's klopt niet. Vernieuw de pagina en probeer het opnieuw.");
		}
	}
}