namespace Sentinel.Ground.Application.Primitives.Results;

public readonly struct Result
{
    private Result(Error? error)
    {
        Error = error;
        IsOk = error is null;
    }

    public bool IsOk { get; }

    public Error Error => field ?? throw new InvalidOperationException("Result is Ok.");

    public static Result Ok() => new(null);

    public static Result Fail(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator Result(Error error) => Fail(error);

    public TR Match<TR>(Func<TR> onOk, Func<Error, TR> onError)
    {
        ArgumentNullException.ThrowIfNull(onOk);
        ArgumentNullException.ThrowIfNull(onError);

        return IsOk ? onOk() : onError(Error);
    }

    public void Match(Action onOk, Action<Error> onError)
    {
        ArgumentNullException.ThrowIfNull(onOk);
        ArgumentNullException.ThrowIfNull(onError);

        if (IsOk)
            onOk();
        else
            onError(Error);
    }
}

public readonly struct Result<T>
{
    private Result(T value)
    {
        Value = value;
        Error = null;
        IsOk = true;
    }

    private Result(Error error)
    {
        Value = default;
        Error = error ?? throw new ArgumentNullException(nameof(error));
        IsOk = false;
    }

    public bool IsOk { get; }

    public T Value => IsOk && field is not null ? field : throw new InvalidOperationException("Result is not Ok.");

    public Error Error => field ?? throw new InvalidOperationException("Result is Ok.");

    public static Result<T> Ok(T value) => new(value);

    public static Result<T> Fail(Error error) => new(error);

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => Fail(error);

    public TR Match<TR>(Func<T, TR> onOk, Func<Error, TR> onError)
    {
        ArgumentNullException.ThrowIfNull(onOk);
        ArgumentNullException.ThrowIfNull(onError);

        return IsOk ? onOk(Value) : onError(Error);
    }

    public void Match(Action<T> onOk, Action<Error> onError)
    {
        ArgumentNullException.ThrowIfNull(onOk);
        ArgumentNullException.ThrowIfNull(onError);

        if (IsOk)
            onOk(Value);
        else
            onError(Error);
    }
}

