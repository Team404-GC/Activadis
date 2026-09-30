using System.ComponentModel.DataAnnotations;

namespace Activadis.Shared.DTOs.Elo.Category
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Naam is verplicht.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
