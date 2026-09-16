namespace Activadis.Shared.DTOs.SignUp
{
    public class SignUpRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public bool HasPlusOne { get; set; }
        public Guid ActivityId { get; set; }
    }
}
