namespace Activadis.Shared.DTOs.Activity
{
    public class ActivitySignUpResponse
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? UserSlug { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Comment { get; set; }
        public bool HasPlusOne { get; set; }
        public bool IsExternal { get; set; }
        public int? WaitlistPosition { get; set; }
    }
}
