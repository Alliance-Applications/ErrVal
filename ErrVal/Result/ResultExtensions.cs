using ErrVal.Option;

namespace ErrVal.Result;

public static class ResultExtensions
{
    public static Result<T> Err<T>(this string message) where T : notnull => new Error(new(message));

    public static Result<T> Err<T>(this Exception message) where T : notnull => new ExceptionError(message);

    public static Result<T> Ok<T>(this T ok) where T : notnull => new(ok);

    public static Result<T> Err<T>(this Error err) where T : notnull => new(err);

    public static Result<T> Guarded<T>(this Func<T> func) where T : notnull
    {
        try
        {
            return func();
        }
        catch (Exception e)
        {
            return new ExceptionError(e);
        }
    }

    public static async Task<Result<T>> GuardedAsync<T>(this Func<Task<T>> func) where T : notnull
    {
        try
        {
            return await func().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return new ExceptionError(e);
        }
    }

    #region Extracting contained values

    public static async Task<T> Expect<T>(this Task<Result<T>> self, string message)
        where T : notnull => (await self.ConfigureAwait(false)).Expect(message);

    public static async Task<T> Unwrap<T>(this Task<Result<T>> self)
        where T : notnull => (await self.ConfigureAwait(false)).Unwrap();

    public static async Task<T> UnwrapOr<T>(this Task<Result<T>> self, T defaultValue)
        where T : notnull => (await self.ConfigureAwait(false)).UnwrapOr(defaultValue);

    public static async Task<T?> UnwrapOrDefault<T>(this Task<Result<T>> self)
        where T : notnull => (await self.ConfigureAwait(false)).UnwrapOrDefault();

    public static async Task<T> UnwrapOrElse<T>(this Task<Result<T>> self, Func<T> func)
        where T : notnull => (await self.ConfigureAwait(false)).UnwrapOrElse(func);

    public static async Task<T> UnwrapOrElse<T>(this Task<Result<T>> self, Func<Task<T>> func)
        where T : notnull => await (await self.ConfigureAwait(false)).UnwrapOrElseAsync(func).ConfigureAwait(false);

    public static async Task<Error> ExpectErr<T>(this Task<Result<T>> self, string message)
        where T : notnull => (await self.ConfigureAwait(false)).ExpectErr(message);

    public static async Task<Error> UnwrapErr<T>(this Task<Result<T>> self)
        where T : notnull => (await self.ConfigureAwait(false)).UnwrapErr();

    public static async Task Match<T>(this Task<Result<T>> self, Action<T> onOk, Action<Error> onErr)
        where T : notnull => (await self.ConfigureAwait(false)).Match(onOk, onErr);

    public static async Task Match<T>(this Task<Result<T>> self, Func<T, Task> onOk, Func<Error, Task> onErr)
        where T : notnull => await (await self.ConfigureAwait(false)).Match(onOk, onErr).ConfigureAwait(false);

    #endregion

    #region Transforming contained values

    public static async Task<Option<T>> Ok<T>(this Task<Result<T>> self)
        where T : notnull => (await self.ConfigureAwait(false)).Ok();

    public static async Task<Option<Error>> Err<T>(this Task<Result<T>> self)
        where T : notnull => (await self.ConfigureAwait(false)).Err();

    public static Option<Result<T>> Transpose<T>(this Result<Option<T>> self)
        where T : notnull => self.err == null ? self.ok!.Val != null ? new(new(self.ok.Val)) : new() : new(new(self.err));

    public static async Task<Option<Result<T>>> Transpose<T>(this Task<Result<Option<T>>> self)
        where T : notnull => (await self.ConfigureAwait(false)).Transpose();

    public static async Task<Result<T>> Inspect<T>(this Task<Result<T>> self, Action<T> func)
        where T : notnull => (await self.ConfigureAwait(false)).Inspect(func);

    public static async Task<Result<T>> Inspect<T>(this Task<Result<T>> self, Func<T, Task> func)
        where T : notnull => await (await self.ConfigureAwait(false)).Inspect(func).ConfigureAwait(false);

    public static async Task<Result<T>> InspectErr<T>(this Task<Result<T>> self, Action<Error> func)
        where T : notnull => (await self.ConfigureAwait(false)).InspectErr(func);

    public static async Task<Result<T>> InspectErr<T>(this Task<Result<T>> self, Func<Error, Task> func)
        where T : notnull => await (await self.ConfigureAwait(false)).InspectErr(func).ConfigureAwait(false);

    public static async Task<Result<TOut>> Map<T, TOut>(this Task<Result<T>> self, Func<T, TOut> func)
        where T : notnull where TOut : notnull => (await self.ConfigureAwait(false)).Map(func);

    public static async Task<Result<TOut>> Map<T, TOut>(this Task<Result<T>> self, Func<T, Task<TOut>> func)
        where T : notnull where TOut : notnull => await (await self.ConfigureAwait(false)).Map(func).ConfigureAwait(false);

    public static async Task<Result<T>> MapErr<T>(this Task<Result<T>> self, Func<Error, Error> func)
        where T : notnull => (await self.ConfigureAwait(false)).MapErr(func);

    public static async Task<Result<T>> MapErr<T>(this Task<Result<T>> self, Func<Error, Task<Error>> func)
        where T : notnull => await (await self.ConfigureAwait(false)).MapErr(func).ConfigureAwait(false);

    public static async Task<TOut> MapOr<T, TOut>(this Task<Result<T>> self, TOut defaultValue, Func<T, TOut> func)
        where T : notnull where TOut : notnull => (await self.ConfigureAwait(false)).MapOr(defaultValue, func);

    public static async Task<TOut> MapOr<T, TOut>(this Task<Result<T>> self, TOut defaultValue, Func<T, Task<TOut>> func)
        where T : notnull where TOut : notnull => await (await self.ConfigureAwait(false)).MapOr(defaultValue, func).ConfigureAwait(false);

    public static async Task<TOut> MapOrElse<T, TOut>(this Task<Result<T>> self, Func<Error, TOut> defaultFunc, Func<T, TOut> func)
        where T : notnull where TOut : notnull => (await self.ConfigureAwait(false)).MapOrElse(defaultFunc, func);

    public static async Task<TOut> MapOrElse<T, TOut>(this Task<Result<T>> self, Func<Error, Task<TOut>> defaultFunc, Func<T, Task<TOut>> func)
        where T : notnull where TOut : notnull => await (await self.ConfigureAwait(false)).MapOrElse(defaultFunc, func).ConfigureAwait(false);

    #endregion

    #region Boolean operators

    public static async Task<Result<TOut>> And<T, TOut>(this Task<Result<T>> self, Result<TOut> other)
        where T : notnull where TOut : notnull => (await self.ConfigureAwait(false)).And(other);

    public static async Task<Result<TOut>> AndThen<T, TOut>(this Task<Result<T>> self, Func<T, Result<TOut>> func)
        where T : notnull where TOut : notnull => (await self.ConfigureAwait(false)).AndThen(func);

    public static async Task<Result<TOut>> AndThen<T, TOut>(this Task<Result<T>> self, Func<T, Task<Result<TOut>>> func)
        where T : notnull where TOut : notnull => await (await self.ConfigureAwait(false)).AndThen(func).ConfigureAwait(false);

    public static async Task<Result<T>> Or<T>(this Task<Result<T>> self, Result<T> other)
        where T : notnull => (await self.ConfigureAwait(false)).Or(other);

    public static async Task<Result<T>> OrElse<T>(this Task<Result<T>> self, Func<Error, Result<T>> func)
        where T : notnull => (await self.ConfigureAwait(false)).OrElse(func);

    public static async Task<Result<T>> OrElse<T>(this Task<Result<T>> self, Func<Error, Task<Result<T>>> func)
        where T : notnull => await (await self.ConfigureAwait(false)).OrElse(func).ConfigureAwait(false);

    #endregion

    public static async Task<Result<T>> AddContext<T>(this Task<Result<T>> self, string message)
        where T : notnull => (await self.ConfigureAwait(false)).AddContext(message);
}