using System.Text;
using System.Text.RegularExpressions;

namespace TestGenerator;

abstract partial class TestSourceBuilder
{
    private readonly StringBuilder _source = new();
    private bool _isStartOfLine;
    private int _indent;
    private bool _suspended;

    public override string ToString()
    {
        return _source.ToString();
    }

    public void WriteLine(string? text = null)
    {
        if (_suspended)
        {
            return;
        }

        if (text != null)
        {
            Write(text);
        }

        _source.AppendLine();
        _isStartOfLine = true;
    }

    public void Write(string text)
    {
        if (_suspended)
        {
            return;
        }

        WriteIndent();
        _source.Append(text);
        _isStartOfLine = false;
    }

    private void WriteIndent()
    {
        if (_isStartOfLine && _indent > 0)
        {
            _source.Append("".PadLeft(4 * _indent, ' '));
            _isStartOfLine = false;
        }
    }

    public void BeginBlock()
    {
        if (_suspended)
        {
            return;
        }

        WriteLine("{");
        _indent++;
    }

    public void EndBlock(bool statement = false)
    {
        if (_suspended)
        {
            return;
        }

        _indent--;
        WriteLine("}" + (statement ? ";" : ""));
    }

    public abstract void WritePartialRenderMethod(string methodName);
    public abstract string MakeEnumAccess(string type, string field);

    public abstract string MakeTestGetProperty(string property);

    public abstract string MakeCallHelperMethod(string methodName, string args, bool statement);
    public abstract string MakeCallTestMethod(string methodName, string args, bool statement);
    public abstract string MakeCallCanvasMethod(string methodName, string args, bool statement);

    public abstract void WriteSetTestProperty(string property, string value);
    public abstract void WriteSetCanvasProperty(string property, string value);

    public abstract string MakeCastToFloat(string value);
    public abstract string MakeCastToByte(double value);

    public string MakeCastToFloat(double value)
    {
        return MakeCastToFloat(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public void WriteCallCanvasMethod(string methodName, string args = "", bool statement = true)
    {
        WriteLine(MakeCallCanvasMethod(methodName, args, statement));
    }

    protected static string ToPascalCase(string s)
    {
        return s[..1].ToUpperInvariant() + s[1..];
    }

    protected static string ToCamelCase(string s)
    {
        return s[..1].ToLowerInvariant() + s[1..];
    }

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", RegexOptions.Compiled)]
    private static partial Regex SnakeCaseRegex();

    protected static string ToSnakeCase(string s)
    {
        s = s.Replace("AlphaSkia", "alphaskia");
        return SnakeCaseRegex().Replace(s, "_$1").Trim().ToLower();
    }

    public virtual string EncodeString(string text)
    {
        return System.Text.Json.JsonSerializer.Serialize(text);
    }

    public void Resume()
    {
        _suspended = false;
    }

    public void Suspend()
    {
        _suspended = true;
    }

    public abstract string MakeStringArray(IList<string> values);
}