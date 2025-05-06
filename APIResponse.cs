
namespace ZoumaFinance
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<T> Errors { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(T data, bool success = true, string message = "", List<T> errors = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors ?? new List<T>();
        }
        public ApiResponse(bool success, string message, List<T> errors = null)
        {
            Success = success;
            Message = message;
            Data = default;
            Errors = errors ?? new List<T>();
        }
    }

}

