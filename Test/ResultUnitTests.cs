using ErrVal.Option;
using ErrVal.Result;

namespace Test;

public class ResultUnitTests
{
    [Fact]
    public void Result_IsOk() => Assert.True(OkData.IsOk);

    [Fact]
    public void Result_IsErr() => Assert.True(ErrData.IsErr);

    [Fact]
    public void Result_CompareTo()
    {
        Assert.Equal(1, ErrData.CompareTo(null));

        Assert.Equal(0, OkData.CompareTo(OkData));
        Assert.Equal(-1, OkData.CompareTo(OkCompare));
        Assert.Equal(1, OkCompare.CompareTo(OkData));

        Assert.Equal(0, ErrData.CompareTo(ErrData));
        Assert.Equal(0, ErrData.CompareTo(ErrCompare));
        Assert.Equal(0, ErrCompare.CompareTo(ErrData));
    }

    [Fact]
    public void Result_ToString()
    {
        Assert.Equal("Ok(TestData { Value = 5 })", OkData.ToString());
        Assert.Equal("Err(TestError { Context = , Message = TEST })", ErrData.ToString());
    }

    [Fact]
    public void Result_Guarded()
    {
        Func<TestData> throws = () => throw new();
        Assert.Equal(Data.Ok(), ResultExtensions.Guarded(ReturnData));
        Assert.True(throws.Guarded() is { IsErr: true, IsOk: false });
    }

    [Fact]
    public async Task Result_GuardedAsync()
    {
        Func<Task<TestData>> throws = () => throw new();
        Assert.Equal(Data.Ok(), await ResultExtensions.GuardedAsync(ReturnDataAsync));
        Assert.True(await throws.GuardedAsync() is { IsErr: true, IsOk: false });
    }

    #region Extracting contained values

    [Fact]
    public async Task Result_Expect()
    {
        Assert.Equal(Data, await AsyncOkData.Expect("TEST"));

        var e = await Assert.ThrowsAsync<NullReferenceException>(
            async () => await AsyncErrData.Expect("TEST")
        );
        Assert.Equal("TEST", e.Message);
    }

    [Fact]
    public async Task Result_Unwrap()
    {
        Assert.Equal(Data, await AsyncOkData.Unwrap());

        var e = await Assert.ThrowsAsync<NullReferenceException>(
            async () => await AsyncErrData.Unwrap()
        );
        Assert.Equal("Object reference not set to an instance of an object.", e.Message);
    }

    [Fact]
    public async Task Result_UnwrapOr()
    {
        Assert.Equal(Data, await AsyncOkData.UnwrapOr(Compare));

        Assert.Equal(Compare, await AsyncErrData.UnwrapOr(Compare));
    }

    [Fact]
    public async Task Result_UnwrapOrDefault()
    {
        Assert.Equal(Data, await AsyncOkData.UnwrapOrDefault());

        Assert.Null(await AsyncErrData.UnwrapOrDefault());
    }

    [Fact]
    public async Task Result_UnwrapOrElse()
    {
        Assert.Equal(Data, await AsyncOkData.UnwrapOrElse(ReturnCompare));

        Assert.Equal(Compare, await AsyncErrData.UnwrapOrElse(ReturnCompare));
    }

    [Fact]
    public async Task Result_UnwrapOrElseAsync()
    {
        Assert.Equal(Data, await AsyncOkData.UnwrapOrElse(ReturnCompareAsync));

        Assert.Equal(Compare, await AsyncErrData.UnwrapOrElse(ReturnCompareAsync));
    }

    [Fact]
    public async Task Result_ExpectErr()
    {
        Assert.Equal(ExceptionData, await AsyncErrData.ExpectErr("TEST"));

        var e = await Assert.ThrowsAsync<NullReferenceException>(
            async () => await AsyncOkData.ExpectErr("TEST")
        );
        Assert.Equal("TEST", e.Message);
    }

    [Fact]
    public async Task Result_UnwrapErr()
    {
        Assert.Equal(ExceptionData, await AsyncErrData.UnwrapErr());

        var e = await Assert.ThrowsAsync<NullReferenceException>(
            async () => await AsyncOkData.UnwrapErr()
        );
        Assert.Equal("Object reference not set to an instance of an object.", e.Message);
    }

    [Fact]
    public async Task Result_Match()
    {
        var i = 0;
        var exception = default(Error);

        await AsyncOkData.Match(ok => i += ok.Value, err => throw new((err as TestError)!.Message));
        await AsyncErrData.Match(ok => Task.Run(() => i += ok.Value), err => Task.Run(() => exception = err));

        Assert.Equal(5, i);
        Assert.Equal(ExceptionData, exception);
    }

    #endregion

    #region Transforming contained values

    [Fact]
    public async Task Result_Ok()
    {
        Assert.Equal(Data.Some(), await AsyncOkData.Ok());
        Assert.Equal(Option<TestData>.None(), await AsyncErrData.Ok());
    }

    [Fact]
    public async Task Result_Err()
    {
        Assert.Equal(Option<Error>.None(), await AsyncOkData.Err());
        Assert.Equal(ExceptionData.Some(), await AsyncErrData.Err());
    }

    [Fact]
    public async Task Result_Transpose()
    {
        var expected = Data.Ok().Some();
        var actual = Task.FromResult(SomeData.Ok());
        Assert.Equal(expected, await actual.Transpose());
    }

    [Fact]
    public async Task Result_Inspect()
    {
        var data = 0;
        Assert.Equal(OkData, await AsyncOkData.Inspect(_ => data += 1));
        Assert.Equal(1, data);

        Assert.Equal(ErrData, await AsyncErrData.Inspect(_ => data += 1));
        Assert.Equal(1, data);
    }

    [Fact]
public async Task Result_InspectAsync()
    {
        var data = 0;
        Assert.Equal(OkData, await AsyncOkData.Inspect(_ => Task.FromResult(data += 1)));
        Assert.Equal(1, data);

        Assert.Equal(ErrData, await AsyncErrData.Inspect(_ => Task.FromResult(data += 1)));
        Assert.Equal(1, data);
    }

    [Fact]
    public async Task Result_InspectErr()
    {
        var data = 0;
        Assert.Equal(OkData, await AsyncOkData.InspectErr(_ => data += 1));
        Assert.Equal(0, data);

        Assert.Equal(ErrData, await AsyncErrData.InspectErr(_ => data += 1));
        Assert.Equal(1, data);
    }

    [Fact]
    public async Task Result_InspectErrAsync()
    {
        var data = 0;
        Assert.Equal(OkData, await AsyncOkData.InspectErr(_ => Task.FromResult(data += 1)));
        Assert.Equal(0, data);

        Assert.Equal(ErrData, await AsyncErrData.InspectErr(_ => Task.FromResult(data += 1)));
        Assert.Equal(1, data);
    }

    [Fact]
    public async Task Result_Map()
    {
        Assert.Equal(OkCompare, await AsyncOkData.Map(_ => Compare));
        Assert.Equal(ErrData, await AsyncErrData.Map(_ => Compare));
    }

    [Fact]
    public async Task Result_MapAsync()
    {
        Assert.Equal(OkCompare, await AsyncOkData.Map(_ => Task.FromResult(Compare)));
        Assert.Equal(ErrData, await AsyncErrData.Map(_ => Task.FromResult(Compare)));
    }

    [Fact]
    public async Task Result_MapErr()
    {
        Assert.Equal(OkData, await AsyncOkData.MapErr(_ => ExceptionCompare));
        Assert.Equal(ErrCompare, await AsyncErrData.MapErr(_ => ExceptionCompare));
    }

    [Fact]
    public async Task Result_MapErrAsync()
    {
        Assert.Equal(OkData, await AsyncOkData.MapErr(_ => Task.FromResult(ExceptionCompare)));
        Assert.Equal(ErrCompare, await AsyncErrData.MapErr(_ => Task.FromResult(ExceptionCompare)));
    }

    [Fact]
    public async Task Result_MapOr()
    {
        Assert.Equal(Data, await AsyncOkData.MapOr(Compare, _ => Data));
        Assert.Equal(Compare, await AsyncErrData.MapOr(Compare, _ => Data));
    }

    [Fact]
    public async Task Result_MapOrAsync()
    {
        Assert.Equal(Data, await AsyncOkData.MapOr(Compare, _ => Task.FromResult(Data)));
        Assert.Equal(Compare, await AsyncErrData.MapOr(Compare, _ => Task.FromResult(Data)));
    }

    [Fact]
    public async Task Result_MapOrElse()
    {
        Assert.Equal(Data, await AsyncOkData.MapOrElse(_ => Compare, _ => Data));
        Assert.Equal(Compare, await AsyncErrData.MapOrElse(_ => Compare, _ => Data));
    }

    [Fact]
    public async Task Result_MapOrElseAsync()
    {
        Assert.Equal(Data, await AsyncOkData.MapOrElse(_ => Task.FromResult(Compare), _ => Task.FromResult(Data)));
        Assert.Equal(Compare, await AsyncErrData.MapOrElse(_ => Task.FromResult(Compare), _ => Task.FromResult(Data)));
    }

    #endregion

    #region Boolean operators

    [Fact]
    public async Task Result_And()
    {
        Assert.Equal(OkCompare, await AsyncOkData.And(OkCompare));
        Assert.Equal(ErrData, await AsyncErrData.And(OkCompare));
    }

    [Fact]
    public async Task Result_AndThen()
    {
        Assert.Equal(OkCompare, await AsyncOkData.AndThen(_ => OkCompare));
        Assert.Equal(ErrData, await AsyncErrData.AndThen(_ => OkCompare));
    }

    [Fact]
    public async Task Result_AndThenAsync()
    {
        Assert.Equal(OkCompare, await AsyncOkData.AndThen(_ => Task.FromResult(OkCompare)));
        Assert.Equal(ErrData, await AsyncErrData.AndThen(_ => Task.FromResult(OkCompare)));
    }

    [Fact]
    public async Task Result_Or()
    {
        Assert.Equal(OkData, await AsyncOkData.Or(OkCompare));
        Assert.Equal(OkCompare, await AsyncErrData.Or(OkCompare));
    }

    [Fact]
    public async Task Result_OrElse()
    {
        Assert.Equal(OkData, await AsyncOkData.OrElse(_ => OkCompare));
        Assert.Equal(OkCompare, await AsyncErrData.OrElse(_ => OkCompare));
    }

    [Fact]
    public async Task Result_OrElseAsync()
    {
        Assert.Equal(OkData, await AsyncOkData.OrElse(_ => Task.FromResult(OkCompare)));
        Assert.Equal(OkCompare, await AsyncErrData.OrElse(_ => Task.FromResult(OkCompare)));
    }

    #endregion
}