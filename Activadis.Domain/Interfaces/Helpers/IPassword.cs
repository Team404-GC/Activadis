namespace Activadis.Domain.Interfaces.Helpers
{
    public interface IPassword
    {
        bool Validate(string hash, string password);
        string Hash(string password);
    }
}
