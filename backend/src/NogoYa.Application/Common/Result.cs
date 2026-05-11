namespace NogoYa.Application.Common;

public class Result
{
    public bool IsSuccess { get; protected init; }
    public string? Error { get; protected init; }
    public string? ErrorCode { get; protected init; }

    protected Result(bool isSuccess, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string? code = null) => new(false, error, code);
    public static Result<T> Success<T>(T value) => Result<T>.SuccessInternal(value);
    public static Result<T> Failure<T>(string error, string? code = null) => Result<T>.FailureInternal(error, code);
}

public class Result<T> : Result
{
    public T? Value { get; private init; }
    private Result(bool isSuccess, T? value, string? error, string? errorCode)
        : base(isSuccess, error, errorCode) => Value = value;

    internal static Result<T> SuccessInternal(T value) => new(true, value, null, null);
    internal static Result<T> FailureInternal(string error, string? code) => new(false, default, error, code);
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
