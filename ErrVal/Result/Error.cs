namespace ErrVal.Result;

public abstract record Error;

public record ExceptionError(Exception Exception) : Error;

public record ValueError<T>(T Value) : Error where T: notnull;