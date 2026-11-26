namespace Lab.Orders.Application.Common;

public enum FailureType
{
    Validation,
    NotFound,
    Conflict,
    Domain
}

public sealed record Failure(FailureType Type, string Code, string Message);

public class Result
{
    protected Result(bool isSuccess, Failure? failure)
    {
        IsSuccess = isSuccess;
        Failure = failure;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Failure? Failure { get; }

    public static Result Ok() => new(true, null);

    public static Result Failed(Failure failure) => new(false, failure);

    public static Result Validation(string code, string message) =>
        Failed(new Failure(FailureType.Validation, code, message));

    public static Result NotFound(string code, string message) =>
        Failed(new Failure(FailureType.NotFound, code, message));

    public static Result Conflict(string code, string message) =>
        Failed(new Failure(FailureType.Conflict, code, message));

    public static Result Domain(string code, string message) =>
        Failed(new Failure(FailureType.Domain, code, message));
}

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, Failure? failure, T? value)
        : base(isSuccess, failure) => Value = value;

    public T? Value { get; }

    public static Result<T> Ok(T value) => new(true, null, value);

    public new static Result<T> Failed(Failure failure) => new(false, failure, default);

    public new static Result<T> Validation(string code, string message) =>
        Failed(new Failure(FailureType.Validation, code, message));

    public new static Result<T> NotFound(string code, string message) =>
        Failed(new Failure(FailureType.NotFound, code, message));

    public new static Result<T> Conflict(string code, string message) =>
        Failed(new Failure(FailureType.Conflict, code, message));

    public new static Result<T> Domain(string code, string message) =>
        Failed(new Failure(FailureType.Domain, code, message));
}
