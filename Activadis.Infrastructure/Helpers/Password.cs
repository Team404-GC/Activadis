using Activadis.Domain.Interfaces.Helpers;

namespace Activadis.Infrastructure.Helpers
{
    public class Password : IPassword
    {
        public bool Validate(string hash, string password)
            => BCrypt.Net.BCrypt.Verify(password, hash);

        public string Hash(string password)
            => BCrypt.Net.BCrypt.HashPassword(password);
    }
}
