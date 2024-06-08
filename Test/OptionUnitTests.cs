using ErrVal;

namespace Test;

/// <summary>
///     Tests for the <see cref="Option{T}" /> type.
///     The async methods are used where possible since they call their synchronous counterparts internally.
/// </summary>
public class OptionUnitTests
{
    [Fact]
    public void Option_Into()
    {
        Assert.Equal(SomeData, Data.Into());
        Assert.Equal(None, default(TestData).Into());
    }

    [Fact]
    public void Option_IsSome() => Assert.True(Data.Some().IsSome);

    [Fact]
    public void Option_IsNone() => Assert.True(Option<TestData>.None().IsNone);

    [Fact]
    public void Option_CompareTo()
    {
        Assert.Equal(1, None.CompareTo(null));

        Assert.Equal(0, SomeData.CompareTo(SomeData));
        Assert.Equal(-1, SomeData.CompareTo(SomeCompare));
        Assert.Equal(1, SomeCompare.CompareTo(SomeData));

        Assert.Equal(0, None.CompareTo(None));
        Assert.Equal(-1, None.CompareTo(SomeData));
        Assert.Equal(1, SomeData.CompareTo(None));
    }

    [Fact]
    public void Option_ToString()
    {
        Assert.Equal("Some(TestData { Value = 5 })", SomeData.ToString());
        Assert.Equal("None", None.ToString());
    }

    #region Extracting the contained value

    [Fact]
    public async Task Option_Expect()
    {
        Assert.Equal(Data, await AsyncSomeData.Expect("This is an error message"));

        var e = await Assert.ThrowsAsync<NullReferenceException>(
            async () => _ = await AsyncNone.Expect("This is an error message")
        );
        Assert.Equal("This is an error message", e.Message);
    }

    [Fact]
    public async Task Option_Unwrap()
    {
        Assert.Equal(Data, await AsyncSomeData.Unwrap());
        var e = await Assert.ThrowsAsync<NullReferenceException>(async () => _ = await AsyncNone.Unwrap());
        Assert.Equal("Object reference not set to an instance of an object.", e.Message);
    }

    [Fact]
    public async Task Option_UnwrapOr()
    {
        Assert.Equal(Data, await AsyncSomeData.UnwrapOr(Compare));
        Assert.Equal(Compare, await AsyncNone.UnwrapOr(Compare));
    }

    [Fact]
    public async Task Option_UnwrapOrDefault()
    {
        Assert.Equal(Data, await AsyncSomeData.UnwrapOrDefault());
        Assert.Null(await AsyncNone.UnwrapOrDefault());
    }

    [Fact]
    public async Task Option_UnwrapOrElse()
    {
        Assert.Equal(Data, await AsyncSomeData.UnwrapOrElse(ReturnCompare));
        Assert.Equal(Compare, await AsyncNone.UnwrapOrElse(ReturnCompare));
    }

    [Fact]
    public async Task Option_UnwrapOrElseAsync()
    {
        Assert.Equal(Data, await AsyncSomeData.UnwrapOrElse(ReturnCompareAsync));
        Assert.Equal(Compare, await AsyncNone.UnwrapOrElse(ReturnCompareAsync));
    }

    #endregion

    #region Transforming the contained values

    [Fact]
    public async Task Option_OkOr()
    {
        Assert.Equal(OkData, await AsyncSomeData.OkOr(ExceptionCompare));
        Assert.Equal(ErrCompare, await AsyncNone.OkOr(ExceptionCompare));
    }

    [Fact]
    public async Task Option_OkOrElse()
    {
        Assert.Equal(OkData, await AsyncSomeData.OkOrElse(ReturnErrorCompare));
        Assert.Equal(ErrCompare, await AsyncNone.OkOrElse(ReturnErrorCompare));
    }

    [Fact]
    public async Task Option_OkOrElseAsync()
    {
        Assert.Equal(OkData, await AsyncSomeData.OkOrElse(ReturnErrorCompareAsync));
        Assert.Equal(ErrCompare, await AsyncNone.OkOrElse(ReturnErrorCompareAsync));
    }

    [Fact]
    public async Task Option_Transpose()
    {
        var optionOfNone = Task.FromResult(Option<Result<TestData, TestError>>.None());
        Assert.Equal(Option<TestData>.None().Ok<Option<TestData>, TestError>(), await optionOfNone.Transpose());

        var optionOfErr = Task.FromResult(ErrData.Some());
        Assert.Equal(ExceptionData.Err<Option<TestData>, TestError>(), await optionOfErr.Transpose());

        var optionOfOk = Task.FromResult(OkData.Some());
        Assert.Equal(Data.Some().Ok<Option<TestData>, TestError>(), await optionOfOk.Transpose());
    }

    [Fact]
    public async Task Option_Flatten()
    {
        var optionOfNone = Task.FromResult(Option<Option<TestData>>.None());
        Assert.Equal(Option<TestData>.None(), await optionOfNone.Flatten());

        var optionOfSomeWithNone = Task.FromResult(Option<TestData>.None().Some());
        Assert.Equal(Option<TestData>.None(), await optionOfSomeWithNone.Flatten());

        var optionOfSomeWithSome = Task.FromResult(SomeData.Some());
        Assert.Equal(SomeData, await optionOfSomeWithSome.Flatten());
    }

    [Fact]
    public async Task Option_Filter()
    {
        Assert.Equal(None, await AsyncSomeData.Filter(_ => false));
        Assert.Equal(SomeData, await AsyncSomeData.Filter(_ => true));
    }

    [Fact]
    public async Task Option_FilterAsync()
    {
        Assert.Equal(None, await AsyncSomeData.Filter(_ => Task.FromResult(false)));
        Assert.Equal(SomeData, await AsyncSomeData.Filter(_ => Task.FromResult(true)));
    }

    [Fact]
    public async Task Option_Inspect()
    {
        var data = default(TestData);
        var outData = await AsyncNone.Inspect(d => data = d);
        Assert.Null(data);
        Assert.Equal(None, outData);

        data = default;
        outData = await AsyncSomeData.Inspect(d => data = d);
        Assert.Equal(Data, data);
        Assert.Equal(SomeData, outData);
    }

    [Fact]
    public async Task Option_InspectAsync()
    {
        var data = default(TestData);
        await AsyncNone.Inspect(d =>
        {
            data = d;
            return Task.CompletedTask;
        });
        Assert.Null(data);

        data = default;
        await AsyncSomeData.Inspect(d =>
        {
            data = d;
            return Task.CompletedTask;
        });
        Assert.Equal(Data, data);
    }

    private static TestData LambdaMap(TestData data) => new(Value: data.Value + 1);

    private static async Task<TestData> LambdaMapAsync(TestData data) =>
        await Task.FromResult(new TestData(data.Value + 1));

    [Fact]
    public async Task Option_Map()
    {
        Assert.Equal(SomeCompare, await AsyncSomeData.Map(LambdaMap));
        Assert.Equal(None, await AsyncNone.Map(LambdaMap));
    }

    [Fact]
    public async Task Option_MapAsync()
    {
        Assert.Equal(SomeCompare, await AsyncSomeData.Map(LambdaMapAsync));
        Assert.Equal(None, await AsyncNone.Map(LambdaMapAsync));
    }

    [Fact]
    public async Task Option_MapOr()
    {
        Assert.Equal(SomeCompare, await AsyncSomeData.MapOr(Compare, LambdaMap));
        Assert.Equal(SomeCompare, await AsyncNone.MapOr(Data, LambdaMap));
    }

    [Fact]
    public async Task Option_MapOrAsync()
    {
        Assert.Equal(SomeCompare, await AsyncSomeData.MapOr(Compare, LambdaMapAsync));
        Assert.Equal(SomeCompare, await AsyncNone.MapOr(Data, LambdaMapAsync));
    }

    [Fact]
    public async Task Option_MapOrElse()
    {
        Assert.Equal(SomeCompare, await AsyncSomeData.MapOrElse(ReturnCompare, LambdaMap));
        Assert.Equal(SomeCompare, await AsyncNone.MapOrElse(ReturnData, LambdaMap));
    }

    [Fact]
    public async Task Option_MapOrElseAsync()
    {
        Assert.Equal(SomeCompare,
            await AsyncSomeData.MapOrElse(ReturnCompareAsync, LambdaMapAsync));
        Assert.Equal(SomeCompare,
            await AsyncNone.MapOrElse(ReturnDataAsync, LambdaMapAsync));
    }

    [Fact]
    public async Task Option_Zip()
    {
        Assert.Equal(Option<(TestData, TestData)>.None(), await AsyncNone.Zip(SomeCompare));
        Assert.Equal(Option<(TestData, TestData)>.None(), await AsyncSomeData.Zip(None));
        Assert.Equal((Data, Compare).Some(), await AsyncSomeData.Zip(SomeCompare));
    }

    private static TestData LambdaZip(TestData d, TestData c) => new(d.Value + c.Value);

    private static async Task<TestData> LambdaZipAsync(TestData d, TestData c) =>
        await Task.FromResult(new TestData(d.Value + c.Value));

    [Fact]
    public async Task Option_ZipWith()
    {
        Assert.Equal(None, await AsyncNone.ZipWith(SomeCompare, LambdaZip));
        Assert.Equal(None, await AsyncSomeData.ZipWith(None, LambdaZip));
        Assert.Equal(new TestData(11).Some(), await AsyncSomeData.ZipWith(SomeCompare,
            LambdaZip
        ));
    }

    [Fact]
    public async Task Option_ZipWithAsync()
    {
        Assert.Equal(None, await AsyncNone.ZipWith(SomeCompare, LambdaZipAsync));
        Assert.Equal(None, await AsyncSomeData.ZipWith(None, LambdaZipAsync));
        Assert.Equal(
            new TestData(11).Some(),
            await AsyncSomeData.ZipWith(SomeCompare, LambdaZipAsync)
        );
    }

    [Fact]
    public async Task Option_Unzip()
    {
        var (left, right) = await AsyncSomeData.Zip(SomeCompare).Unzip();
        Assert.Equal(SomeData, left);
        Assert.Equal(SomeCompare, right);
    }

    [Fact]
    public async Task Option_UnzipAsync()
    {
        var (left, right) = await AsyncSomeData.Zip(SomeCompare).Unzip();
        Assert.Equal(SomeData, left);
        Assert.Equal(SomeCompare, right);
    }

    [Fact]
    public async Task Option_FlatMap()
    {
        Assert.Equal(None, await AsyncNone.FlatMap(_ => SomeCompare));
        Assert.Equal(SomeCompare, await AsyncSomeData.FlatMap(_ => SomeCompare));
    }

    [Fact]
    public async Task Option_FlatMapAsync()
    {
        Assert.Equal(None, await AsyncNone.FlatMap(_ => Task.FromResult(SomeCompare)));
        Assert.Equal(SomeCompare, await AsyncSomeData.FlatMap(_ => Task.FromResult(SomeCompare)));
    }

    #endregion

    #region Boolean operators

    [Fact]
    public async Task Option_And()
    {
        Assert.Equal(None, await AsyncNone.And(None));
        Assert.Equal(None, await AsyncNone.And(SomeData));
        Assert.Equal(None, await AsyncSomeData.And(None));
        Assert.Equal(SomeData, await AsyncSomeData.And(SomeData));
    }

    [Fact]
    public async Task Option_AndThen()
    {
        Assert.Equal(None, await AsyncNone.AndThen(_ => None));
        Assert.Equal(None, await AsyncNone.AndThen(_ => SomeCompare));
        Assert.Equal(None, await AsyncSomeData.AndThen(_ => None));
        Assert.Equal(SomeData, await AsyncSomeData.AndThen(_ => SomeCompare));
    }

    [Fact]
    public async Task Option_AndThenAsync()
    {
        Assert.Equal(None, await AsyncNone.AndThen(_ => AsyncNone));
        Assert.Equal(None, await AsyncNone.AndThen(_ => AsyncSomeCompare));
        Assert.Equal(None, await AsyncSomeData.AndThen(_ => AsyncNone));
        Assert.Equal(SomeCompare, await AsyncSomeData.AndThen(_ => AsyncSomeCompare));
    }

    [Fact]
    public async Task Option_Or()
    {
        Assert.Equal(None, await AsyncNone.Or(None));
        Assert.Equal(SomeData, await AsyncNone.Or(SomeData));
        Assert.Equal(SomeData, await AsyncSomeData.Or(None));
        Assert.Equal(SomeData, await AsyncSomeData.Or(SomeCompare));
    }

    [Fact]
    public async Task Option_OrElse()
    {
        Assert.Equal(None, await AsyncNone.OrElse(() => None));
        Assert.Equal(SomeData, await AsyncNone.OrElse(() => SomeData));
        Assert.Equal(SomeData, await AsyncSomeData.OrElse(() => None));
        Assert.Equal(SomeData, await AsyncSomeData.OrElse(() => SomeCompare));
    }

    [Fact]
    public async Task Option_OrElseAsync()
    {
        Assert.Equal(None, await AsyncNone.OrElse(() => AsyncNone));
        Assert.Equal(SomeData, await AsyncNone.OrElse(() => AsyncSomeData));
        Assert.Equal(SomeData, await AsyncSomeData.OrElse(() => AsyncNone));
        Assert.Equal(SomeData, await AsyncSomeData.OrElse(() => AsyncSomeCompare));
    }

    [Fact]
    public async Task Option_Xor()
    {
        Assert.Equal(None, await AsyncNone.Xor(None));
        Assert.Equal(SomeCompare, await AsyncNone.Xor(SomeCompare));
        Assert.Equal(SomeData, await AsyncSomeData.Xor(None));
        Assert.Equal(None, await AsyncSomeData.Xor(SomeCompare));
    }

    #endregion
}