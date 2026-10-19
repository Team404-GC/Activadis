namespace Activadis.Shared.DTOs.SignUp
{
    public class ResignUpRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool HasPlusOne { get; set; }
        public Guid ActivityId { get; set; }
    }
}
