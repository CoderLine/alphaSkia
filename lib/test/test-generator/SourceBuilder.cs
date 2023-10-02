using System.Text;

namespace TestGenerator;

abstract class SourceBuilder
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
        if(_suspended)
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
        if(_suspended)
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
        if(_suspended)
        {
            return;
        }
        WriteLine("{");
        _indent++;
    }

    public void EndBlock(bool statement = false)
    {
        if(_suspended)
        {
            return;
        }
        _indent--;
        WriteLine("}" + (statement ? ";" : ""));
    }

    public abstract string MakeGetProperty(string property);
    public abstract string MakeMethodAccess(string expression, string member);
    public abstract string MakeMethodAccess(string member);

    public abstract void WriteSetProperty(string property, string value);
    public abstract void WriteSetCanvasProperty(string property, string value);

    public abstract string UnicodeEscape(int x);
    public abstract string MakeEnumAccess(string type, string field);


    public void WriteCallCanvasMethod(string methodName, string args = "", bool statement = true)
    {
        WriteLine(this.MakeCallMethod(MakeMethodAccess("canvas", methodName), args, statement));
    }

    public string MakeCallMethod(string methodName, string args = "", bool statement = true)
    {
        return $"{methodName}({args}){(statement ? ";" : "")}";
    }

    public string MakeCallStaticMethod(string typeName, string methodName, string args = "", bool statement = true)
    {
        return $"{MakeMethodAccess(typeName, methodName)}({args}){(statement ? ";" : "")}";
    }

    protected static string ToPascalCase(string s)
    {
        return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
    }

    protected string ToCamelCase(string s)
    {
        return s.Substring(0, 1).ToLowerInvariant() + s.Substring(1);
    }

    public string EncodeString(string text)
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

class CSharpSourceBuilder : SourceBuilder
{
    private string MakePropertyAccess(string expression, string member)
    {
        return $"{expression}.{ToPascalCase(member)}";
    }

    public override string MakeMethodAccess(string member)
    {
        return ToPascalCase(member);
    }

    public override string MakeMethodAccess(string expression, string member)
    {
        return $"{expression}.{ToPascalCase(member)}";
    }

    public override void WriteSetProperty(string property, string value)
    {
        WriteLine($"{ToPascalCase(property)} = {value};");
    }

    public override string MakeGetProperty(string property)
    {
        return ToPascalCase(property);
    }

    public override string MakeEnumAccess(string type, string field)
    {
        return $"{type}.{ToPascalCase(field)}";
    }

    public override string UnicodeEscape(int x)
    {
        return $"\\x{x:X4}";
    }

    public override void WriteSetCanvasProperty(string property, string value)
    {
        WriteLine($"{MakePropertyAccess("canvas", property)} = {value};");
    }
}

class JavaSourceBuilder : SourceBuilder
{
    public override string MakeMethodAccess(string member)
    {
        return ToCamelCase(member);
    }

    public override string MakeMethodAccess(string expression, string member)
    {
        return $"{expression}.{ToCamelCase(member)}";
    }

    public override void WriteSetProperty(string property, string value)
    {
        WriteLine($"set{ToPascalCase(property)}({value});");
    }

    public override string MakeGetProperty(string property)
    {
        return $"get{ToPascalCase(property)}()";
    }

    public override string MakeEnumAccess(string type, string field)
    {
        return $"{type}.{field.ToUpperInvariant()}";
    }

    public override string UnicodeEscape(int x)
    {
        return $"\\u{x:XXXX}";
    }

    public override void WriteSetCanvasProperty(string property, string value)
    {
        WriteLine($"canvas.set{ToPascalCase(property)}({value});");
    }
}