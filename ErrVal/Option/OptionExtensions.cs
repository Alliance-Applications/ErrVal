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
        where T : notnull => (await self).Expect(message);

    public static async Task<T> Unwrap<T>(this Task<Option<T>> self)
        where T : notnull => (await self).Unwrap();

    [Pure]
    public static async Task<T> UnwrapOr<T>(this Task<Option<T>> self, T other)
        where T : notnull => (await self).UnwrapOr(other);

    [Pure]
    public static async Task<T?> UnwrapOrDefault<T>(this Task<Option<T>> self)
        where T : notnull => (await self).UnwrapOrDefault();

    public static async Task<T> UnwrapOrElse<T>(this Task<Option<T>> self, Func<T> other)
        where T : notnull => (await self).UnwrapOrElse(other);

    public static async Task<T> UnwrapOrElse<T>(this Task<Option<T>> self, Func<Task<T>> other)
        where T : notnull => await (await self).UnwrapOrElse(other);

    #endregion

    #region Transforming the contained values

    public static async Task<Result<T, TErr>> OkOr<T, TErr>(this Task<Option<T>> self, TErr err)
        where T : notnull where TErr : notnull => (await self).OkOr(err);

    public static async Task<Result<T, TErr>> OkOrElse<T, TErr>(this Task<Option<T>> self, Func<TErr> func)
        where T : notnull where TErr : notnull => (await self).OkOrElse(func);

    public static async Task<Result<T, TErr>> OkOrElse<T, TErr>(this Task<Option<T>> self, Func<Task<TErr>> func)
        where T : notnull where TErr : notnull => await (await self).OkOrElse(func);

    public static Result<Option<T>, TErr> Transpose<T, TErr>(this Option<Result<T, TErr>> self)
        where T : notnull where TErr : notnull => self.Val == null
        ? Option<T>.None().Ok<Option<T>, TErr>()
        : self.Val.err is { } e
            ? e.Err<Option<T>, TErr>()
            : Some(self.Val.ok!).Ok<Option<T>, TErr>();

    public static async Task<Result<Option<T>, TErr>> Transpose<T, TErr>(this Task<Option<Result<T, TErr>>> self)
        where T : notnull where TErr : notnull => (await self).Transpose();

    public static Option<T> Flatten<T>(this Option<Option<T>> self)
        where T : notnull => self.Val ?? new();

    public static async Task<Option<T>> Flatten<T>(this Task<Option<Option<T>>> self)
        where T : notnull => (await self).Flatten();

    public static async Task<Option<T>> Filter<T>(this Task<Option<T>> self, Func<T, bool> func)
        where T : notnull => (await self).Filter(func);

    public static async Task<Option<T>> Filter<T>(this Task<Option<T>> self, Func<T, Task<bool>> func)
        where T : notnull => await (await self).Filter(func);

    public static async Task<Option<T>> Inspect<T>(this Task<Option<T>> self, Action<T> action)
        where T : notnull => (await self).Inspect(action);

    public static async Task<Option<T>> Inspect<T>(this Task<Option<T>> self, Func<T, Task> action)
        where T : notnull => await (await self).Inspect(action);

    public static async Task<Option<TOut>> Map<TIn, TOut>(this Task<Option<TIn>> self, Func<TIn, TOut> func)
        where TIn : notnull where TOut : notnull => (await self).Map(func);

    public static async Task<Option<TOut>> Map<TIn, TOut>(this Task<Option<TIn>> self, Func<TIn, Task<TOut>> func)
        where TIn : notnull where TOut : notnull => await (await self).Map(func);

    public static async Task<Option<TOut>> MapOr<TIn, TOut>
        (this Task<Option<TIn>> self, TIn other, Func<TIn, TOut> func)
        where TIn : notnull where TOut : notnull => (await self).MapOr(other, func);

    public static async Task<Option<TOut>> MapOr<TIn, TOut>
        (this Task<Option<TIn>> self, TIn other, Func<TIn, Task<TOut>> func)
        where TIn : notnull where TOut : notnull => await (await self).MapOr(other, func);

    public static async Task<Option<TOut>> MapOrElse<TIn, TOut>
        (this Task<Option<TIn>> self, Func<TIn> other, Func<TIn, TOut> func)
        where TIn : notnull where TOut : notnull => (await self).MapOrElse(other, func);

    public static async Task<Option<TOut>> MapOrElse<TIn, TOut>
        (this Task<Option<TIn>> self, Func<Task<TIn>> other, Func<TIn, Task<TOut>> func)
        where TIn : notnull where TOut : notnull => await (await self).MapOrElse(other, func);

    public static async Task<Option<(T, TOther)>> Zip<T, TOther>(this Task<Option<T>> self, Option<TOther> other)
        where T : notnull where TOther : notnull => (await self).Zip(other);

    public static async Task<Option<TOut>> ZipWith<T, TOther, TOut>
        (this Task<Option<T>> self, Option<TOther> other, Func<T, TOther, TOut> func)
        where T : notnull where TOther : notnull where TOut : notnull => (await self).ZipWith(other, func);

    public static async Task<Option<TOut>> ZipWith<T, TOther, TOut>
        (this Task<Option<T>> self, Option<TOther> other, Func<T, TOther, Task<TOut>> func)
        where T : notnull where TOther : notnull where TOut : notnull => await (await self).ZipWith(other, func);

    public static (Option<TOut>, Option<TOther>) Unzip<TOut, TOther>(this Option<(TOut, TOther)> self)
        where TOut : notnull where TOther : notnull =>
        self.Val is ({ } output, { } other) ? (new(output), new(other)) : new();

    public static async Task<(Option<TOut>, Option<TOther>)> Unzip<TOut, TOther>(this Task<Option<(TOut, TOther)>> self)
        where TOut : notnull where TOther : notnull => (await self).Unzip();

    public static async Task<Option<TOut>> FlatMap<T, TOut>(this Task<Option<T>> self, Func<T, Option<TOut>> func)
        where T : notnull where TOut : notnull => (await self).FlatMap(func);

    public static async Task<Option<TOut>> FlatMap<T, TOut>(this Task<Option<T>> self, Func<T, Task<Option<TOut>>> func)
        where T : notnull where TOut : notnull => await (await self).FlatMap(func);

    #endregion

    #region Boolean operators

    public static async Task<Option<TOut>> And<TOut>(this Task<Option<TOut>> self, Option<TOut> other)
        where TOut : notnull => (await self).And(other);

    public static async Task<Option<TOut>> AndThen<TOut>(this Task<Option<TOut>> self, Func<TOut, Option<TOut>> func)
        where TOut : notnull => (await self).AndThen(func);

    public static async Task<Option<TOut>> AndThen<TOut>
        (this Task<Option<TOut>> self, Func<TOut, Task<Option<TOut>>> func)
        where TOut : notnull => await (await self).AndThen(func);

    public static async Task<Option<TOut>> Or<TOut>(this Task<Option<TOut>> self, Option<TOut> other)
        where TOut : notnull => (await self).Or(other);

    public static async Task<Option<TOut>> OrElse<TOut>(this Task<Option<TOut>> self, Func<Option<TOut>> func)
        where TOut : notnull => (await self).OrElse(func);

    public static async Task<Option<TOut>> OrElse<TOut>(this Task<Option<TOut>> self, Func<Task<Option<TOut>>> func)
        where TOut : notnull => await (await self).OrElse(func);

    public static async Task<Option<TOut>> Xor<TOut>(this Task<Option<TOut>> self, Option<TOut> other)
        where TOut : notnull => (await self).Xor(other);

    #endregion

    public static async Task Match<T>(this Task<Option<T>> self, Action<T> onSome, Action onNone)
        where T : notnull => (await self).Match(onSome, onNone);

    public static async Task Match<T>(this Task<Option<T>> self, Func<T, Task> onSome, Func<Task> onNone)
        where T : notnull => await (await self).Match(onSome, onNone);
}