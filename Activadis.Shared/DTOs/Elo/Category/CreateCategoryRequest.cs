namespace Activadis.Shared.DTOs.Elo.Category
{
    public class CreateCategoryRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }
}
