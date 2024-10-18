using System.Diagnostics.Contracts;
using ErrVal.Result;

namespace ErrVal.Option;

public static class OptionExtensions
{
    [Pure]
    public static Option<T> Some<T>(this T some) where T : notnull => new(some);

    [Pure]
    public static Option<T> Into<T>(this T? value) where T : notnull => new(value);

    #region Extracting the contained value

    public static async Task<T> Expect<T>(this Task<Option<T>> self, string message)
        where T : notnull => (await self.ConfigureAwait(false)).Expect(message);

    public static async Task<T> Unwrap<T>(this Task<Option<T>> self)
        where T : notnull => (await self.ConfigureAwait(false)).Unwrap();

    [Pure]
    public static async Task<T> UnwrapOr<T>(this Task<Option<T>> self, T other)
        where T : notnull => (await self.ConfigureAwait(false)).UnwrapOr(other);

    [Pure]
    public static async Task<T?> UnwrapOrDefault<T>(this Task<Option<T>> self)
        where T : notnull => (await self.ConfigureAwait(false)).UnwrapOrDefault();

    public static async Task<T> UnwrapOrElse<T>(this Task<Option<T>> self, Func<T> other)
        where T : notnull => (await self.ConfigureAwait(false)).UnwrapOrElse(other);

    public static async Task<T> UnwrapOrElse<T>(this Task<Option<T>> self, Func<Task<T>> other)
        where T : notnull => await (await self.ConfigureAwait(false)).UnwrapOrElse(other).ConfigureAwait(false);

    #endregion

    #region Transforming the contained values

    public static async Task<Result<T>> OkOr<T>(this Task<Option<T>> self, Error err)
        where T : notnull => (await self.ConfigureAwait(false)).OkOr(err);

    public static async Task<Result<T>> OkOrElse<T>(this Task<Option<T>> self, Func<Error> func)
        where T : notnull => (await self.ConfigureAwait(false)).OkOrElse(func);

    public static async Task<Result<T>> OkOrElse<T>(this Task<Option<T>> self, Func<Task<Error>> func)
        where T : notnull => await (await self.ConfigureAwait(false)).OkOrElse(func).ConfigureAwait(false);

    public static Result<Option<T>> Transpose<T>(this Option<Result<T>> self)
        where T : notnull => self.Val == null
        ? Option<T>.None().Ok()
        : self.Val.err is { } e
            ? e.Err<Option<T>>()
            : Some(self.Val.ok!).Ok();

    public static async Task<Result<Option<T>>> Transpose<T>(this Task<Option<Result<T>>> self)
        where T : notnull => (await self.ConfigureAwait(false)).Transpose();

    public static Option<T> Flatten<T>(this Option<Option<T>> self)
        where T : notnull => self.Val ?? new();

    public static async Task<Option<T>> Flatten<T>(this Task<Option<Option<T>>> self)
        where T : notnull => (await self.ConfigureAwait(false)).Flatten();

    public static async Task<Option<T>> Filter<T>(this Task<Option<T>> self, Func<T, bool> func)
        where T : notnull => (await self.ConfigureAwait(false)).Filter(func);

    public static async Task<Option<T>> Filter<T>(this Task<Option<T>> self, Func<T, Task<bool>> func)
        where T : notnull => await (await self.ConfigureAwait(false)).Filter(func).ConfigureAwait(false);

    public static async Task<Option<T>> Inspect<T>(this Task<Option<T>> self, Action<T> action)
        where T : notnull => (await self.ConfigureAwait(false)).Inspect(action);

    public static async Task<Option<T>> Inspect<T>(this Task<Option<T>> self, Func<T, Task> action)
        where T : notnull => await (await self.ConfigureAwait(false)).Inspect(action).ConfigureAwait(false);

    public static async Task<Option<TOut>> Map<TIn, TOut>(this Task<Option<TIn>> self, Func<TIn, TOut> func)
        where TIn : notnull where TOut : notnull => (await self.ConfigureAwait(false)).Map(func);

    public static async Task<Option<TOut>> Map<TIn, TOut>(this Task<Option<TIn>> self, Func<TIn, Task<TOut>> func)
        where TIn : notnull where TOut : notnull => await (await self.ConfigureAwait(false)).Map(func).ConfigureAwait(false);

    public static async Task<Option<TOut>> MapOr<TIn, TOut>
        (this Task<Option<TIn>> self, TIn other, Func<TIn, TOut> func)
        where TIn : notnull where TOut : notnull => (await self.ConfigureAwait(false)).MapOr(other, func);

    public static async Task<Option<TOut>> MapOr<TIn, TOut>
        (this Task<Option<TIn>> self, TIn other, Func<TIn, Task<TOut>> func)
        where TIn : notnull where TOut : notnull => await (await self.ConfigureAwait(false)).MapOr(other, func).ConfigureAwait(false);

    public static async Task<Option<TOut>> MapOrElse<TIn, TOut>
        (this Task<Option<TIn>> self, Func<TIn> other, Func<TIn, TOut> func)
        where TIn : notnull where TOut : notnull => (await self.ConfigureAwait(false)).MapOrElse(other, func);

    public static async Task<Option<TOut>> MapOrElse<TIn, TOut>
        (this Task<Option<TIn>> self, Func<Task<TIn>> other, Func<TIn, Task<TOut>> func)
        where TIn : notnull where TOut : notnull => await (await self.ConfigureAwait(false)).MapOrElse(other, func).ConfigureAwait(false);

    public static async Task<Option<(T, TOther)>> Zip<T, TOther>(this Task<Option<T>> self, Option<TOther> other)
        where T : notnull where TOther : notnull => (await self.ConfigureAwait(false)).Zip(other);

    public static async Task<Option<TOut>> ZipWith<T, TOther, TOut>
        (this Task<Option<T>> self, Option<TOther> other, Func<T, TOther, TOut> func)
        where T : notnull where TOther : notnull where TOut : notnull => (await self.ConfigureAwait(false)).ZipWith(other, func);

    public static async Task<Option<TOut>> ZipWith<T, TOther, TOut>
        (this Task<Option<T>> self, Option<TOther> other, Func<T, TOther, Task<TOut>> func)
        where T : notnull where TOther : notnull where TOut : notnull => await (await self.ConfigureAwait(false)).ZipWith(other, func).ConfigureAwait(false);

    public static (Option<TOut>, Option<TOther>) Unzip<TOut, TOther>(this Option<(TOut, TOther)> self)
        where TOut : notnull where TOther : notnull =>
        self.Val is ({ } output, { } other) ? (new(output), new(other)) : new();

    public static async Task<(Option<TOut>, Option<TOther>)> Unzip<TOut, TOther>(this Task<Option<(TOut, TOther)>> self)
        where TOut : notnull where TOther : notnull => (await self.ConfigureAwait(false)).Unzip();

    public static async Task<Option<TOut>> FlatMap<T, TOut>(this Task<Option<T>> self, Func<T, Option<TOut>> func)
        where T : notnull where TOut : notnull => (await self.ConfigureAwait(false)).FlatMap(func);

    public static async Task<Option<TOut>> FlatMap<T, TOut>(this Task<Option<T>> self, Func<T, Task<Option<TOut>>> func)
        where T : notnull where TOut : notnull => await (await self.ConfigureAwait(false)).FlatMap(func).ConfigureAwait(false);

    #endregion

    #region Boolean operators

    public static async Task<Option<TOut>> And<TOut>(this Task<Option<TOut>> self, Option<TOut> other)
        where TOut : notnull => (await self.ConfigureAwait(false)).And(other);

    public static async Task<Option<TOut>> AndThen<TOut>(this Task<Option<TOut>> self, Func<TOut, Option<TOut>> func)
        where TOut : notnull => (await self.ConfigureAwait(false)).AndThen(func);

    public static async Task<Option<TOut>> AndThen<TOut>
        (this Task<Option<TOut>> self, Func<TOut, Task<Option<TOut>>> func)
        where TOut : notnull => await (await self.ConfigureAwait(false)).AndThen(func).ConfigureAwait(false);

    public static async Task<Option<TOut>> Or<TOut>(this Task<Option<TOut>> self, Option<TOut> other)
        where TOut : notnull => (await self.ConfigureAwait(false)).Or(other);

    public static async Task<Option<TOut>> OrElse<TOut>(this Task<Option<TOut>> self, Func<Option<TOut>> func)
        where TOut : notnull => (await self.ConfigureAwait(false)).OrElse(func);

    public static async Task<Option<TOut>> OrElse<TOut>(this Task<Option<TOut>> self, Func<Task<Option<TOut>>> func)
        where TOut : notnull => await (await self.ConfigureAwait(false)).OrElse(func).ConfigureAwait(false);

    public static async Task<Option<TOut>> Xor<TOut>(this Task<Option<TOut>> self, Option<TOut> other)
        where TOut : notnull => (await self.ConfigureAwait(false)).Xor(other);

    #endregion
}