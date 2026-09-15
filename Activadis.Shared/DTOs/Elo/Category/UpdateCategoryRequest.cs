namespace Activadis.Shared.DTOs.Elo.Category
{
    public class UpdateCategoryRequest
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
    }
}
