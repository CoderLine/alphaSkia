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
}

class CSharpTestSourceBuilder : TestSourceBuilder
{
    public override void WritePartialRenderMethod(string methodName)
    {
        WriteLine($"private AlphaSkiaImage? {ToPascalCase(methodName)}(AlphaSkiaCanvas canvas)");
    }

    public override string MakeCallTestMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"{ToPascalCase(methodName)}", args, statement);
    }

    public override string MakeCallHelperMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"AlphaSkiaCanvas.{ToPascalCase(methodName)}", args, statement);
    }

    public override string MakeCallCanvasMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"canvas.{ToPascalCase(methodName)}", args, statement);
    }

    public override string MakeCastToFloat(string value)
    {
        return $"(float)({value})";
    }

    public override string MakeCastToByte(double value)
    {
        return $"(byte)({value})";
    }

    private string MakeCallMethod(string methodName, string args = "", bool statement = true,
        bool notNull = false)
    {
        return $"{methodName}({args}){(notNull ? "!" : "")}{(statement ? ";" : "")}";
    }

    private string MakePropertyAccess(string expression, string member)
    {
        return $"{expression}.{ToPascalCase(member)}";
    }

    public override void WriteSetTestProperty(string property, string value)
    {
        WriteLine($"{ToPascalCase(property)} = {value};");
    }

    public override string MakeTestGetProperty(string property)
    {
        return ToPascalCase(property);
    }

    public override string MakeEnumAccess(string type, string field)
    {
        return $"{type}.{ToPascalCase(field)}";
    }

    public override void WriteSetCanvasProperty(string property, string value)
    {
        WriteLine($"{MakePropertyAccess("canvas", property)} = {value};");
    }
}

class JavaTestSourceBuilder : TestSourceBuilder
{
    public override void WritePartialRenderMethod(string methodName)
    {
        Write($"private AlphaSkiaImage {ToCamelCase(methodName)}(AlphaSkiaCanvas canvas) ");
    }

    public override string MakeCallTestMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"{ToCamelCase(methodName)}", args, statement);
    }

    public override string MakeCallCanvasMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"canvas.{ToCamelCase(methodName)}", args, statement);
    }

    public override string MakeCallHelperMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"AlphaSkiaCanvas.{ToCamelCase(methodName)}", args, statement);
    }

    private string MakeCallMethod(string methodName, string args = "", bool statement = true)
    {
        return $"{methodName}({args}){(statement ? ";" : "")}";
    }

    public override string MakeCastToFloat(string value)
    {
        return $"(float)({value})";
    }

    public override string MakeCastToByte(double value)
    {
        return $"(byte)({value})";
    }

    public override void WriteSetTestProperty(string property, string value)
    {
        WriteLine($"set{ToPascalCase(property)}({value});");
    }

    public override string MakeTestGetProperty(string property)
    {
        return $"get{ToPascalCase(property)}()";
    }

    public override string MakeEnumAccess(string type, string field)
    {
        return $"{type}.{field.ToUpperInvariant()}";
    }

    public override void WriteSetCanvasProperty(string property, string value)
    {
        WriteLine($"canvas.set{ToPascalCase(property)}({value});");
    }
}

class TypeScriptTestSourceBuilder : TestSourceBuilder
{
    public override void WritePartialRenderMethod(string methodName)
    {
        Write($"function {ToCamelCase(methodName)}(canvas: AlphaSkiaCanvas): AlphaSkiaImage ");
    }

    public override string MakeCallTestMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"test.{ToCamelCase(methodName)}", args, statement);
    }

    public override string MakeCallHelperMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"AlphaSkiaCanvas.{ToCamelCase(methodName)}", args, statement);
    }

    public override string MakeCallCanvasMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"canvas.{ToCamelCase(methodName)}", args, statement);
    }

    private string MakeCallMethod(string methodName, string args = "", bool statement = true,
        bool notNull = false)
    {
        return $"{methodName}({args}){(notNull ? "!" : "")}{(statement ? ";" : "")}";
    }

    public override string MakeCastToFloat(string value)
    {
        return value;
    }

    public override string MakeCastToByte(double value)
    {
        return ((int)value).ToString();
    }

    private string MakePropertyAccess(string expression, string member)
    {
        return $"{expression}.{ToCamelCase(member)}";
    }

    public override void WriteSetTestProperty(string property, string value)
    {
        WriteLine($"test.{ToCamelCase(property)} = {value};");
    }

    public override string MakeTestGetProperty(string property)
    {
        return $"test.{ToCamelCase(property)}";
    }

    public override string MakeEnumAccess(string type, string field)
    {
        return $"{type}.{ToPascalCase(field)}";
    }

    public override void WriteSetCanvasProperty(string property, string value)
    {
        WriteLine($"{MakePropertyAccess("canvas", property)} = {value};");
    }
}

class CppTestSourceBuilder : TestSourceBuilder
{
    public override void WritePartialRenderMethod(string methodName)
    {
        Write($"alphaskia_image_t {ToSnakeCase(methodName)}(alphaskia_canvas_t canvas) ");
    }

    public override string MakeCallTestMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"alphaskia_{ToSnakeCase(methodName)}", args, statement);
    }

    public override string MakeCallHelperMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"alphaskia_{ToSnakeCase(methodName)}", args, statement);
    }

    public override string MakeCallCanvasMethod(string methodName, string args, bool statement)
    {
        args = args.Length > 0 ? "canvas, " + args : "canvas";
        return MakeCallMethod($"alphaskia_canvas_{ToSnakeCase(methodName)}",  args, statement);
    }

    private string MakeCallMethod(string methodName, string args = "", bool statement = true)
    {
        return $"{methodName}({args}){(statement ? ";" : "")}";
    }

    public override string MakeCastToFloat(string value)
    {
        return $"static_cast<float>({value})";
    }

    public override string MakeCastToByte(double value)
    {
        return $"static_cast<uint8_t>({(byte)value})";
    }

    public override void WriteSetTestProperty(string property, string value)
    {
        WriteLine($"{ToSnakeCase(property)} = {value};");
    }

    public override string MakeTestGetProperty(string property)
    {
        return $"{ToSnakeCase(property)}";
    }

    public override string MakeEnumAccess(string type, string field)
    {
        return $"{ToSnakeCase(type)}_{field.ToLower()}";
    }

    public override void WriteSetCanvasProperty(string property, string value)
    {
        WriteLine($"alphaskia_canvas_set_{ToSnakeCase(property)}(canvas, {value});");
    }

    public override string EncodeString(string text)
    {
        return "u" + base.EncodeString(text);
    }
}