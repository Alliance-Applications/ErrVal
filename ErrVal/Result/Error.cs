using System.Text;

namespace ErrVal.Result;

public record Error(Context? Context = null);

public record ExceptionError(Exception Exception) : Error;

public record ValueError<T>(T Value) : Error where T: notnull;

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