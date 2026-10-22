namespace Activadis.Shared.DTOs
{
    public class ApiResponse<T>
    {
        public T? Value { get; set; }
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public List<ApiErrors> Errors { get; set; } = [];

        public static ApiResponse<T> Ok(T? value = default, string? message = null)
            => new ApiResponse<T>() { Succeeded = true, Message = message, Value = value };

        public static ApiResponse<T> Fail(string? message = null)
            => new ApiResponse<T>() { Succeeded = false, Message = message };

        public static ApiResponse<T> Error(List<ApiErrors> errors, string? message = "Er zijn automatische fouten opgetreden.")
            => new ApiResponse<T>() { Succeeded = false, Errors = errors, Message = message };
    }
}
