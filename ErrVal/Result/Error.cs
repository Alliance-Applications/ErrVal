using System.Text;

namespace ErrVal.Result;

public record Error(Context? Context = null)
{
    public override string ToString() => Context is null ? "Error" : $"Error\n{Context}";
}

public record ExceptionError(Exception Exception) : Error
{
    public override string ToString() => Context is null ? $"Error\n{Exception}\n" : $"Error\n{Exception}\n{Context}";
}

public record ValueError<T>(T Value) : Error where T : notnull
{
    public override string ToString() => Context is null ? $"Error(Value = {Value})" : $"Error(Value = {Value})\n{Context}";
}

public record Context(string Message, Context? SubContext = null)
{
    public override string ToString()
    {
        var b = new StringBuilder();
        BuildString(b);
        return b.ToString();
    }

    private void BuildString(StringBuilder b)
    {
        b.Append(SubContext == null ? "\u2514\u2500\u2500 " : "\u251c\u2500\u2500 ").Append(Message).AppendLine();
        SubContext?.BuildString(b);
    }
}