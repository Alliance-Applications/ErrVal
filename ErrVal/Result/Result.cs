using System.Diagnostics.Contracts;
using ErrVal.Option;

namespace ErrVal.Result;

[Pure]
public record Result<T> : IComparable<Result<T>> where T : notnull
{
    internal readonly Error? err;
    internal readonly T? ok;

    internal Result(T ok) => this.ok = ok;

    internal Result(Error err) => this.err = err;

    [Pure] public bool IsOk => ok != null;

    [Pure] public bool IsErr => err != null;

    public int CompareTo(Result<T>? other) => other == null
        ? 1
        : other.err is { } e
            ? err == null ? -1 : err is IComparable<Error> comparable ? comparable.CompareTo(e) : 0
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
    public async Task<T> UnwrapOrElseAsync(Func<Task<T>> func) => ok ?? await func().ConfigureAwait(false);

    public Error ExpectErr(string message) => err ?? throw new NullReferenceException(message);
    public Error UnwrapErr() => err ?? throw new NullReferenceException();

    public void Match(Action<T> onOk, Action<Error> onErr)
    {
        switch (ok)
        {
            case not null: onOk(ok); break;
            case null: onErr(err!); break;
        }
    }

    public async Task Match(Func<T, Task> onOk, Func<Error, Task> onErr)
    {
        switch (ok)
        {
            case not null: await onOk(ok).ConfigureAwait(false); break;
            case null: await onErr(err!).ConfigureAwait(false); break;
        }
    }

    #endregion

    #region Transforming contained values

    [Pure]
    public Option<T> Ok() => new(ok);

    [Pure]
    public Option<Error> Err() => new(err);

    public Result<T> Inspect(Action<T> func)
    {
        if (ok != null) func(ok);
        return this;
    }

    public async Task<Result<T>> Inspect(Func<T, Task> func)
    {
        if (ok != null) await func(ok).ConfigureAwait(false);
        return this;
    }

    public Result<T> InspectErr(Action<Error> func)
    {
        if (err != null) func(err);
        return this;
    }

    public async Task<Result<T>> InspectErr(Func<Error, Task> func)
    {
        if (err != null) await func(err).ConfigureAwait(false);
        return this;
    }

    public Result<TOut> Map<TOut>(Func<T, TOut> func) where TOut : notnull
        => ok is null ? new(err!) : new(func(ok));

    public async Task<Result<TOut>> Map<TOut>(Func<T, Task<TOut>> func) where TOut : notnull
        => ok is null ? new(err!) : new(await func(ok).ConfigureAwait(false));

    public Result<T> MapErr(Func<Error, Error> func)
        => ok != null ? new(ok) : new(func(err!));

    public async Task<Result<T>> MapErr(Func<Error, Task<Error>> func)
        => ok != null ? new(ok) : new(await func(err!).ConfigureAwait(false));

    public TOut MapOr<TOut>(TOut defaultValue, Func<T, TOut> func)
        => ok != null ? func(ok) : defaultValue;

    public async Task<TOut> MapOr<TOut>(TOut defaultValue, Func<T, Task<TOut>> func)
        => ok != null ? await func(ok).ConfigureAwait(false) : defaultValue;

    public TOut MapOrElse<TOut>(Func<Error, TOut> defaultFunc, Func<T, TOut> func)
        => ok != null ? func(ok) : defaultFunc(err!);

    public async Task<TOut> MapOrElse<TOut>(Func<Error, Task<TOut>> defaultFunc, Func<T, Task<TOut>> func)
        => ok != null ? await func(ok).ConfigureAwait(false) : await defaultFunc(err!).ConfigureAwait(false);

    #endregion

    #region Boolean operators

    public Result<TOut> And<TOut>(Result<TOut> other)
        where TOut : notnull => err != null ? new(err) : other;

    public Result<TOut> AndThen<TOut>(Func<T, Result<TOut>> func)
        where TOut : notnull => err != null ? new(err) : func(ok!);

    public async Task<Result<TOut>> AndThen<TOut>(Func<T, Task<Result<TOut>>> func)
        where TOut : notnull => err != null ? new(err) : await func(ok!).ConfigureAwait(false);

    public Result<T> Or(Result<T> other) => ok != null ? new(ok) : other;

    public Result<T> OrElse(Func<Error, Result<T>> func) => ok != null ? new(ok) : func(err!);

    public async Task<Result<T>> OrElse(Func<Error, Task<Result<T>>> func) => ok != null ? new(ok) : await func(err!).ConfigureAwait(false);

    #endregion
}