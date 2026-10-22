namespace Activadis.Shared.DTOs.SignUp
{
    public class EditSignUpRequest
    {
        public string? Email { get; set; }
        public bool HasPlusOne { get; set; }
        public Guid ActivityId { get; set; }
    }
}
