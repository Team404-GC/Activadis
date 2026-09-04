namespace Activadis.Application.DTOs
{
    public class ApiResponse<T>
    {
        public T? Value { get; set; }
        public bool Succeeded { get; set; }
        public string? Message { get; set; }

        public static ApiResponse<T> Ok(T? value = default, string? message = null)
            => new ApiResponse<T>() { Succeeded = true, Message = message, Value = value };

        public static ApiResponse<T> Fail(string? message = null)
            => new ApiResponse<T>() { Succeeded = false, Message = message };
    }
}
