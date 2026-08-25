namespace SmartStock.Application.Common;

public class ResultModel<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ResultModel<T> Ok(T data, string message = "Success")
        => new() { Success = true, Message = message, Data = data };

    public static ResultModel<T> Fail(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Data = default, Errors = errors };
}
