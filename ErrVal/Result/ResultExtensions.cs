using ErrVal.Option;

namespace ErrVal.Result;

public static class ResultExtensions
{
    public static Result<T, TErr> Ok<T, TErr>(this T ok)
        where T : notnull
        where TErr : notnull =>
        new(ok);

    public static Result<T, TErr> Err<T, TErr>(this TErr err)
        where T : notnull
        where TErr : notnull =>
        new(err);

    public static Result<T, Exception> Guarded<T>(this Func<T> func) where T : notnull
    {
        try
        {
            return func().Ok<T, Exception>();
        }
        catch (Exception e)
        {
            return e.Err<T, Exception>();
        }
    }

    public static async Task<Result<T, Exception>> GuardedAsync<T>(this Func<Task<T>> func) where T : notnull
    {
        try
        {
            return (await func()).Ok<T, Exception>();
        }
        catch (Exception e)
        {
            return e.Err<T, Exception>();
        }
    }

    #region Extracting contained values

    public static async Task<T> Expect<T, TErr>(this Task<Result<T, TErr>> self, string message)
        where T : notnull where TErr : notnull => (await self).Expect(message);

    public static async Task<T> Unwrap<T, TErr>(this Task<Result<T, TErr>> self)
        where T : notnull where TErr : notnull => (await self).Unwrap();

    public static async Task<T> UnwrapOr<T, TErr>(this Task<Result<T, TErr>> self, T defaultValue)
        where T : notnull where TErr : notnull => (await self).UnwrapOr(defaultValue);

    public static async Task<T?> UnwrapOrDefault<T, TErr>(this Task<Result<T, TErr>> self)
        where T : notnull where TErr : notnull => (await self).UnwrapOrDefault();

    public static async Task<T> UnwrapOrElse<T, TErr>(this Task<Result<T, TErr>> self, Func<T> func)
        where T : notnull where TErr : notnull => (await self).UnwrapOrElse(func);

    public static async Task<T> UnwrapOrElse<T, TErr>(this Task<Result<T, TErr>> self, Func<Task<T>> func)
        where T : notnull where TErr : notnull => await (await self).UnwrapOrElseAsync(func);

    public static async Task<TErr> ExpectErr<T, TErr>(this Task<Result<T, TErr>> self, string message)
        where T : notnull where TErr : notnull => (await self).ExpectErr(message);

    public static async Task<TErr> UnwrapErr<T, TErr>(this Task<Result<T, TErr>> self)
        where T : notnull where TErr : notnull => (await self).UnwrapErr();

    #endregion

    #region Transforming contained values

    public static async Task<Option<T>> Ok<T, TErr>(this Task<Result<T, TErr>> self)
        where T : notnull where TErr : notnull => (await self).Ok();

    public static async Task<Option<TErr>> Err<T, TErr>(this Task<Result<T, TErr>> self)
        where T : notnull where TErr : notnull => (await self).Err();

    public static Option<Result<T, TErr>> Transpose<T, TErr>(this Result<Option<T>, TErr> self)
        where T : notnull where TErr : notnull
        => self.err == null ? self.ok!.Val != null ? new(new(self.ok.Val)) : new() : new(new(self.err));

    public static async Task<Option<Result<T, TErr>>> Transpose<T, TErr>(this Task<Result<Option<T>, TErr>> self)
        where T : notnull where TErr : notnull => (await self).Transpose();

    public static async Task<Result<T, TErr>> Inspect<T, TErr>(this Task<Result<T, TErr>> self, Action<T> func)
        where T : notnull where TErr : notnull => (await self).Inspect(func);

    public static async Task<Result<T, TErr>> Inspect<T, TErr>(this Task<Result<T, TErr>> self, Func<T, Task> func)
        where T : notnull where TErr : notnull => await (await self).Inspect(func);

    public static async Task<Result<T, TErr>> InspectErr<T, TErr>(this Task<Result<T, TErr>> self, Action<TErr> func)
        where T : notnull where TErr : notnull => (await self).InspectErr(func);

    public static async Task<Result<T, TErr>> InspectErr<T, TErr>(this Task<Result<T, TErr>> self,
        Func<TErr, Task> func)
        where T : notnull where TErr : notnull => await (await self).InspectErr(func);

    public static async Task<Result<TOut, TErr>> Map<T, TOut, TErr>
        (this Task<Result<T, TErr>> self, Func<T, TOut> func)
        where T : notnull where TOut : notnull where TErr : notnull => (await self).Map(func);

    public static async Task<Result<TOut, TErr>> Map<T, TOut, TErr>
        (this Task<Result<T, TErr>> self, Func<T, Task<TOut>> func)
        where T : notnull where TOut : notnull where TErr : notnull => await (await self).Map(func);

    public static async Task<Result<T, TOut>> MapErr<T, TOut, TErr>
        (this Task<Result<T, TErr>> self, Func<TErr, TOut> func)
        where T : notnull where TOut : notnull where TErr : notnull => (await self).MapErr(func);

    public static async Task<Result<T, TOut>> MapErr<T, TOut, TErr>
        (this Task<Result<T, TErr>> self, Func<TErr, Task<TOut>> func)
        where T : notnull where TOut : notnull where TErr : notnull => await (await self).MapErr(func);

    public static async Task<TOut> MapOr<T, TErr, TOut>
        (this Task<Result<T, TErr>> self, TOut defaultValue, Func<T, TOut> func)
        where T : notnull where TOut : notnull where TErr : notnull => (await self).MapOr(defaultValue, func);

    public static async Task<TOut> MapOr<T, TErr, TOut>
        (this Task<Result<T, TErr>> self, TOut defaultValue, Func<T, Task<TOut>> func)
        where T : notnull where TOut : notnull where TErr : notnull => await (await self).MapOr(defaultValue, func);

    public static async Task<TOut> MapOrElse<T, TErr, TOut>
        (this Task<Result<T, TErr>> self, Func<TErr, TOut> defaultFunc, Func<T, TOut> func)
        where T : notnull where TOut : notnull where TErr : notnull => (await self).MapOrElse(defaultFunc, func);

    public static async Task<TOut> MapOrElse<T, TErr, TOut>
        (this Task<Result<T, TErr>> self, Func<TErr, Task<TOut>> defaultFunc, Func<T, Task<TOut>> func)
        where T : notnull where TOut : notnull where TErr : notnull => await (await self).MapOrElse(defaultFunc, func);

    #endregion

    #region Boolean operators

    public static async Task<Result<TOut, TErr>> And<T, TOut, TErr>
        (this Task<Result<T, TErr>> self, Result<TOut, TErr> other)
        where T : notnull where TOut : notnull where TErr : notnull => (await self).And(other);

    public static async Task<Result<TOut, TErr>> AndThen<T, TOut, TErr>
        (this Task<Result<T, TErr>> self, Func<T, Result<TOut, TErr>> func)
        where T : notnull where TOut : notnull where TErr : notnull => (await self).AndThen(func);

    public static async Task<Result<TOut, TErr>> AndThen<T, TOut, TErr>
        (this Task<Result<T, TErr>> self, Func<T, Task<Result<TOut, TErr>>> func)
        where T : notnull where TOut : notnull where TErr : notnull => await (await self).AndThen(func);

    public static async Task<Result<T, TErrOut>> Or<T, TErr, TErrOut>
        (this Task<Result<T, TErr>> self, Result<T, TErrOut> other)
        where T : notnull where TErr : notnull where TErrOut : notnull => (await self).Or(other);

    public static async Task<Result<T, TErrOut>> OrElse<T, TErr, TErrOut>
        (this Task<Result<T, TErr>> self, Func<TErr, Result<T, TErrOut>> func)
        where T : notnull where TErr : notnull where TErrOut : notnull => (await self).OrElse(func);

    public static async Task<Result<T, TErrOut>> OrElse<T, TErr, TErrOut>
        (this Task<Result<T, TErr>> self, Func<TErr, Task<Result<T, TErrOut>>> func)
        where T : notnull where TErr : notnull where TErrOut : notnull => await (await self).OrElse(func);

    #endregion

    public static async Task Match<T, TErr>(this Task<Result<T, TErr>> self, Action<T> onOk, Action<TErr> onErr)
        where T : notnull where TErr : notnull => (await self).Match(onOk, onErr);

    public static async Task Match<T, TErr>(this Task<Result<T, TErr>> self, Func<T, Task> onOk, Func<TErr, Task> onErr)
        where T : notnull where TErr : notnull => await (await self).Match(onOk, onErr);
}