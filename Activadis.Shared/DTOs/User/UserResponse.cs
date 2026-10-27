namespace Activadis.Shared.DTOs.User
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string? Slug { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
