using System.ComponentModel.DataAnnotations;

namespace Activadis.Shared.DTOs.SignUp
{
    public abstract class HasCommentBase
    {
        [MaxLength(500, ErrorMessage = "De opmerking mag maximaal maar {1} karakters bevatten.")]
        public virtual string? Comment { get; set; }
    }
}
