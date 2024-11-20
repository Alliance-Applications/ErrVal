namespace ErrVal.Unit;

/// <summary>
/// A type that represents a value that can be used to indicate that a function has no return value.
///
/// This type is similar to the `void` type in C# but can be used as a generic type parameter. Useful for Result types.
/// It has its own namespace due to conflicts with other libraries, for example MediatR.
/// </summary>
public sealed record Unit;