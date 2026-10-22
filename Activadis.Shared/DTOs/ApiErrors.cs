namespace Activadis.Shared.DTOs
{
    public class ApiErrors
    {
        public string Key { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = [];

        public static ApiErrors Build(string key, List<string> errors)
            => new ApiErrors() { Key = key, Errors = errors };
    }
}
