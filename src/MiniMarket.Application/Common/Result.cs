namespace MiniMarket.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }

    protected Result(bool isSuccess, string? error, IReadOnlyList<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? (error is not null ? [error] : []);
    }

    public static Result Success() => new(true, null);

    public static Result Failure(string error) => new(false, error);

    public static Result Failure(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        return new Result(false, errorList.FirstOrDefault(), errorList);
    }

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);

    public static Result<T> Failure<T>(IEnumerable<string> errors) => Result<T>.Failure(errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T? value, bool isSuccess, string? error, IReadOnlyList<string>? errors = null)
        : base(isSuccess, error, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, null);

    public new static Result<T> Failure(string error) => new(default, false, error);

    public new static Result<T> Failure(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        return new Result<T>(default, false, errorList.FirstOrDefault(), errorList);
    }

    public static implicit operator Result<T>(T value) => Success(value);
}
