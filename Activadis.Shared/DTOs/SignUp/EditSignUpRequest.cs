namespace Activadis.Shared.DTOs.SignUp
{
    public class EditSignUpRequest : HasCommentBase
    {
        public string? Email { get; set; }
        public bool HasPlusOne { get; set; }
        public Guid ActivityId { get; set; }
    }
}
