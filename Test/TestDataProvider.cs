using ErrVal.Option;
using ErrVal.Result;

namespace Test;

internal static class TestDataProvider
{
    internal static TestData Data => new(5);
    internal static TestData Compare => new(6);

    internal static Error ExceptionData => new TestError("TEST");
    internal static Error ExceptionCompare => new TestError("COMPARE");

    internal static TestData ReturnData() => Data;
    internal static TestData ReturnCompare() => Compare;
    internal static Error ReturnErrorData() => ExceptionData;
    internal static Error ReturnErrorCompare() => ExceptionCompare;

    internal static Option<TestData> ReturnNone() => None;
    internal static Option<TestData> ReturnSome() => SomeData;
    internal static Option<TestData> ReturnSomeCmp() => SomeCompare;

    internal static Task<Option<TestData>> ReturnNoneA() => Task.FromResult(None);
    internal static Task<Option<TestData>> ReturnSomeA() => Task.FromResult(SomeData);
    internal static Task<Option<TestData>> ReturnSomeCmpA() => Task.FromResult(SomeCompare);

    internal static Task<TestData> ReturnDataAsync() => Task.FromResult(Data);
    internal static Task<TestData> ReturnCompareAsync() => Task.FromResult(Compare);
    internal static Task<Error> ReturnErrorDataAsync() => Task.FromResult(ExceptionData);
    internal static Task<Error> ReturnErrorCompareAsync() => Task.FromResult(ExceptionCompare);

    internal static Option<TestData> SomeData => Data.Some();
    internal static Option<TestData> SomeCompare => Compare.Some();
    internal static Option<TestData> None => Option<TestData>.None();

    internal static Task<Option<TestData>> AsyncSomeData => Task.FromResult(Data.Some());
    internal static Task<Option<TestData>> AsyncSomeCompare => Task.FromResult(Compare.Some());
    internal static Task<Option<TestData>> AsyncNone => Task.FromResult(Option<TestData>.None());

    internal static Result<TestData> OkData => Data.Ok();
    internal static Result<TestData> ErrData => ExceptionData.Err<TestData>();
    internal static Result<TestData> OkCompare => Compare.Ok();
    internal static Result<TestData> ErrCompare => ExceptionCompare.Err<TestData>();

    internal static Task<Result<TestData>> AsyncOkData => Task.FromResult(OkData);
    internal static Task<Result<TestData>> AsyncErrData => Task.FromResult(ErrData);
    internal static Task<Result<TestData>> AsyncOkCompare => Task.FromResult(OkCompare);
    internal static Task<Result<TestData>> AsyncErrCompare => Task.FromResult(ErrCompare);

    internal record TestData(int Value) : IComparable<TestData>
    {
        public int CompareTo(TestData? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Value.CompareTo(other.Value);
        }

        public virtual bool Equals(TestData? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override int GetHashCode() => Value.GetHashCode();
    }

    internal sealed record TestError(string Message) : Error, IComparable<TestError>
    {
        public int CompareTo(TestError? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Message, other.Message, StringComparison.Ordinal);
        }

        public bool Equals(TestError? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Message, other.Message, StringComparison.Ordinal);
        }

        public override int GetHashCode() => Message.GetHashCode();
    }
}