namespace Activadis.Application.DTOs.Auth
{
    public class Token
    {
        public string JWT { get; set; } = string.Empty;
        public DateTime ExpiresOn { get; set; }
    }
}
