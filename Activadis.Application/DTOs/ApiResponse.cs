namespace Activadis.Application.DTOs
{
    public class ApiResponse<T>
    {
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public T? Value { get; set; }

        public static ApiResponse<T> Ok(T? value, string? message)
            => new ApiResponse<T>() { Succeeded = true, Message = message, Value = value };

        public static ApiResponse<T> Fail(string? message)
            => new ApiResponse<T>() { Succeeded = false, Message = message };
    }
}
