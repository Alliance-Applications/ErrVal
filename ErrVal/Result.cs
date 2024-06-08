using System.Diagnostics.Contracts;

namespace ErrVal;

[Pure]
public record Result<T, TErr> : IComparable<Result<T, TErr>> where T : notnull where TErr : notnull
{
    internal readonly TErr? err;
    internal readonly T? ok;

    internal Result(T ok) => this.ok = ok;

    internal Result(TErr err) => this.err = err;

    [Pure] public bool IsOk => ok != null;

    [Pure] public bool IsErr => err != null;

    public int CompareTo(Result<T, TErr>? other) => other == null
        ? 1
        : other.err is { } e
            ? err == null ? -1 : err is IComparable<TErr> comparable ? comparable.CompareTo(e) : 0
            : ok == null
                ? 1
                : ok is IComparable<T> cmp
                    ? cmp.CompareTo(other.ok!)
                    : 0;

    public override string ToString() => ok != null ? $"Ok({ok})" : $"Err({err})";

    #region Extracting contained values

    public T Expect(string message) => ok ?? throw new NullReferenceException(message);
    public T Unwrap() => ok ?? throw new NullReferenceException();

    [Pure]
    public T UnwrapOr(T defaultValue) => ok ?? defaultValue;

    [Pure]
    public T? UnwrapOrDefault() => ok ?? default;

    public T UnwrapOrElse(Func<T> func) => ok ?? func();
    public async Task<T> UnwrapOrElseAsync(Func<Task<T>> func) => ok ?? await func();

    public TErr ExpectErr(string message) => err ?? throw new NullReferenceException(message);
    public TErr UnwrapErr() => err ?? throw new NullReferenceException();

    #endregion

    #region Transforming contained values

    [Pure]
    public Option<T> Ok() => new(ok);

    [Pure]
    public Option<TErr> Err() => new(err);

    public Result<T, TErr> Inspect(Action<T> func)
    {
        if (ok != null) func(ok);
        return this;
    }

    public async Task<Result<T, TErr>> Inspect(Func<T, Task> func)
    {
        if (ok != null) await func(ok);
        return this;
    }

    public Result<T, TErr> InspectErr(Action<TErr> func)
    {
        if (err != null) func(err);
        return this;
    }

    public async Task<Result<T, TErr>> InspectErr(Func<TErr, Task> func)
    {
        if (err != null) await func(err);
        return this;
    }

    public Result<TOut, TErr> Map<TOut>(Func<T, TOut> func)
        where TOut : notnull => ok != null ? new(func(ok)) : new(err!);

    public async Task<Result<TOut, TErr>> Map<TOut>(Func<T, Task<TOut>> func)
        where TOut : notnull => ok != null ? new(await func(ok)) : new(err!);

    public Result<T, TOut> MapErr<TOut>(Func<TErr, TOut> func)
        where TOut : notnull => ok != null ? new(ok) : new(func(err!));

    public async Task<Result<T, TOut>> MapErr<TOut>(Func<TErr, Task<TOut>> func)
        where TOut : notnull => ok != null ? new(ok) : new(await func(err!));

    public TOut MapOr<TOut>(TOut defaultValue, Func<T, TOut> func)
        => ok != null ? func(ok) : defaultValue;

    public async Task<TOut> MapOr<TOut>(TOut defaultValue, Func<T, Task<TOut>> func)
        => ok != null ? await func(ok) : defaultValue;

    public TOut MapOrElse<TOut>(Func<TErr, TOut> defaultFunc, Func<T, TOut> func)
        => ok != null ? func(ok) : defaultFunc(err!);

    public async Task<TOut> MapOrElse<TOut>(Func<TErr, Task<TOut>> defaultFunc, Func<T, Task<TOut>> func)
        => ok != null ? await func(ok) : await defaultFunc(err!);

    #endregion

    #region Boolean operators

    public Result<TOut, TErr> And<TOut>(Result<TOut, TErr> other)
        where TOut : notnull => err != null ? new(err) : other;

    public Result<TOut, TErr> AndThen<TOut>(Func<T, Result<TOut, TErr>> func)
        where TOut : notnull => err != null ? new(err) : func(ok!);

    public async Task<Result<TOut, TErr>> AndThen<TOut>(Func<T, Task<Result<TOut, TErr>>> func)
        where TOut : notnull => err != null ? new(err) : await func(ok!);

    public Result<T, TErrOut> Or<TErrOut>(Result<T, TErrOut> other)
        where TErrOut : notnull => ok != null ? new(ok) : other;

    public Result<T, TErrOut> OrElse<TErrOut>(Func<TErr, Result<T, TErrOut>> func)
        where TErrOut : notnull => ok != null ? new(ok) : func(err!);

    public async Task<Result<T, TErrOut>> OrElse<TErrOut>(Func<TErr, Task<Result<T, TErrOut>>> func)
        where TErrOut : notnull => ok != null ? new(ok) : await func(err!);

    #endregion
}