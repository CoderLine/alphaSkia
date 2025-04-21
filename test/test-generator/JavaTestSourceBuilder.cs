using System.Text.Json;

namespace TestGenerator;

class JavaTestSourceBuilder : TestSourceBuilder
{
    public override void WritePartialRenderMethod(string methodName)
    {
        Write($"private static AlphaSkiaImage {ToCamelCase(methodName)}(AlphaSkiaCanvas canvas) ");
    }

    public override string MakeCallTestMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"{ToCamelCase(methodName)}", args, statement);
    }

    public override string MakeCallHelperMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"AlphaSkiaCanvas.{ToCamelCase(methodName)}", args, statement);
    }

    public override string MakeCallCanvasMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"canvas.{ToCamelCase(methodName)}", args, statement);
    }

    public override string MakeCastToFloat(string value)
    {
        return $"(float)({value})";
    }

    public override string MakeCastToByte(double value)
    {
        return $"(byte)({value})";
    }

    private string MakeCallMethod(string methodName, string args = "", bool statement = true)
    {
        return $"{methodName}({args}){(statement ? ";" : "")}";
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
        return $"{type}.{ToSnakeCase(field).ToUpperInvariant()}";
    }

    public override void WriteSetCanvasProperty(string property, string value)
    {
        WriteLine($"canvas.set{ToPascalCase(property)}({value});");
    }
    
    protected override bool SupportsUtf32EscapeSequence => false;
    
    public override string MakeStringArray(IList<string> values)
    {
        var items = string.Join(", ", values.Select(v => JsonSerializer.Serialize(v)));
        return "{" + items + "}";
    }
}