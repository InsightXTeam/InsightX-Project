namespace InsightX.Application.Common
{
    public class ServiceResult
    {
        public bool IsSuccess { get; init; }
        public int StatusCode { get; init; }   // 200, 400, 401, 404, etc.
        public string? Error { get; init; }

        public static ServiceResult Success() => new() { IsSuccess = true, StatusCode = 200 };
        public static ServiceResult Fail(int code, string error) => new() { IsSuccess = false, StatusCode = code, Error = error };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; init; }

        public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, StatusCode = 200, Data = data };
        public static new ServiceResult<T> Fail(int code, string error) => new() { IsSuccess = false, StatusCode = code, Error = error };
    }
}
