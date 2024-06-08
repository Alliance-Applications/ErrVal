using System.Diagnostics.Contracts;

namespace ErrVal;

[Pure]
public record Option<T> : IComparable<Option<T>> where T : notnull
{
    internal readonly T? Val;

    internal Option(T? val = default) => Val = val;

    [Pure] public bool IsSome => Val != null;

    [Pure] public bool IsNone => Val == null;

    [Pure]
    public int CompareTo(Option<T>? other) =>
        other == null
            ? 1
            : Val == null
                ? other.Val == null ? 0 : -1
                : other.Val == null
                    ? 1
                    : Val is IComparable<T> comparable
                        ? comparable.CompareTo(other.Val)
                        : 0;

    public override string ToString() => Val?.ToString() is { } s ? $"Some({s})" : "None";

    [Pure]
    public static Option<T> None() => new();

    #region Extracting the contained value

    /// <summary>
    ///     Returns the value if it's not null; otherwise, throws a <see cref="NullReferenceException" /> with the specified
    ///     message.
    /// </summary>
    /// <param name="message">The message to include in the exception if the value is null.</param>
    /// <returns>The value of type <typeparamref name="T" /> if it is not null.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value is null.</exception>
    public T Expect(string message) => Val ?? throw new NullReferenceException(message);

    /// <summary>
    ///     Returns the value if it's not null; otherwise, throws a <see cref="NullReferenceException" />.
    /// </summary>
    /// <returns>The value of type <typeparamref name="T" /> if it is not null.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value is null.</exception>
    public T Unwrap() => Val ?? throw new NullReferenceException();

    /// <summary>
    ///     Returns the value if it's not null; otherwise, returns the specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the value is null.</param>
    /// <returns>The value of type <typeparamref name="T" /> if it is not null; otherwise, the specified default value.</returns>
    [Pure]
    public T UnwrapOr(T defaultValue) => Val ?? defaultValue;

    /// <summary>
    ///     Returns the value if it's not null; otherwise, returns the default value for the type <typeparamref name="T" />.
    /// </summary>
    /// <returns>
    ///     The value of type <typeparamref name="T" /> if it is not null; otherwise, the default value for the type
    ///     <typeparamref name="T" />.
    /// </returns>
    [Pure]
    public T? UnwrapOrDefault() => Val ?? default;

    /// <summary>
    ///     Returns the value if it's not null; otherwise, invokes the specified function and returns its result.
    /// </summary>
    /// <param name="defaultFunc">The function to invoke if the value is null.</param>
    /// <returns>
    ///     The value of type <typeparamref name="T" /> if it is not null; otherwise, the result of the specified
    ///     function.
    /// </returns>
    public T UnwrapOrElse(Func<T> defaultFunc) => Val ?? defaultFunc();

    public async Task<T> UnwrapOrElse(Func<Task<T>> defaultFunc) => Val ?? await defaultFunc();

    #endregion

    #region Transforming the contained values

    [Pure]
    public Result<T, TErr> OkOr<TErr>(TErr err)
        where TErr : notnull =>
        Val?.Ok<T, TErr>() ?? err.Err<T, TErr>();

    public Result<T, TErr> OkOrElse<TErr>(Func<TErr> func)
        where TErr : notnull =>
        Val?.Ok<T, TErr>() ?? func().Err<T, TErr>();

    public async Task<Result<T, TErr>> OkOrElse<TErr>(Func<Task<TErr>> func)
        where TErr : notnull =>
        Val?.Ok<T, TErr>() ?? (await func()).Err<T, TErr>();

    public Option<T> Filter(Func<T, bool> func) => Val is not null && !func(Val) ? new() : this;

    public async Task<Option<T>> Filter(Func<T, Task<bool>> func) => Val is not null && !await func(Val) ? new() : this;

    public Option<T> Inspect(Action<T> action)
    {
        if (Val != null) action(Val);
        return this;
    }

    public async Task<Option<T>> Inspect(Func<T, Task> action)
    {
        if (Val != null) await action(Val);
        return this;
    }

    public Option<TOut> Map<TOut>(Func<T, TOut> func)
        where TOut : notnull =>
        new(Val != null ? func(Val) : default);

    public async Task<Option<TOut>> Map<TOut>(Func<T, Task<TOut>> func)
        where TOut : notnull =>
        new(Val != null ? await func(Val) : default);

    public Option<TOut> MapOr<TOut>(T defaultValue, Func<T, TOut> func)
        where TOut : notnull =>
        new(func(Val ?? defaultValue));

    public async Task<Option<TOut>> MapOr<TOut>(T defaultValue, Func<T, Task<TOut>> func)
        where TOut : notnull =>
        new(await func(Val ?? defaultValue));

    public Option<TOut> MapOrElse<TOut>(Func<T> defaultFunc, Func<T, TOut> func)
        where TOut : notnull =>
        new(func(Val ?? defaultFunc()));

    public async Task<Option<TOut>> MapOrElse<TOut>(Func<Task<T>> defaultFunc, Func<T, Task<TOut>> func)
        where TOut : notnull =>
        new(await func(Val ?? await defaultFunc()));

    [Pure]
    public Option<(T, TOut)> Zip<TOut>(Option<TOut> other)
        where TOut : notnull =>
        Val != null && other.Val != null ? new((Val, other.Val)) : new();

    public Option<TOut> ZipWith<TOther, TOut>(Option<TOther> other, Func<T, TOther, TOut> func)
        where TOut : notnull
        where TOther : notnull =>
        Val != null && other.Val != null ? new(func(Val, other.Val)) : new();

    public async Task<Option<TOut>> ZipWith<TOther, TOut>(Option<TOther> other, Func<T, TOther, Task<TOut>> func)
        where TOut : notnull
        where TOther : notnull =>
        Val != null && other.Val != null ? new(await func(Val, other.Val)) : new();

    public Option<TOut> FlatMap<TOut>(Func<T, Option<TOut>> func)
        where TOut : notnull =>
        Val != null ? func(Val) : new();

    public async Task<Option<TOut>> FlatMap<TOut>(Func<T, Task<Option<TOut>>> func)
        where TOut : notnull =>
        Val != null ? await func(Val) : new();

    #endregion

    #region Boolean operators

    [Pure]
    public Option<TOut> And<TOut>(Option<TOut> other)
        where TOut : notnull =>
        Val == null ? new() : other;

    public Option<TOut> AndThen<TOut>(Func<T, Option<TOut>> func)
        where TOut : notnull =>
        Val == null ? new() : func(Val);

    public async Task<Option<TOut>> AndThen<TOut>(Func<T, Task<Option<TOut>>> func)
        where TOut : notnull =>
        Val == null ? new() : await func(Val);

    [Pure]
    public Option<T> Or(Option<T> other) => Val != null ? this : other;

    public Option<T> OrElse(Func<Option<T>> func) => Val == null ? func() : this;

    public async Task<Option<T>> OrElse(Func<Task<Option<T>>> func) => Val == null ? await func() : this;

    [Pure]
    public Option<T> Xor(Option<T> other) => other.Val == null ? this : Val == null ? other : new();

    #endregion
}