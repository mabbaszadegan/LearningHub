namespace EduTrack.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public object? Data { get; }

    private Result(bool isSuccess, string? error, object? data = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Data = data;
    }

    public static Result Success() => new(true, null);
    public static Result Success(object data) => new(true, null, data);
    public static Result Failure(string error) => new(false, error);
}
