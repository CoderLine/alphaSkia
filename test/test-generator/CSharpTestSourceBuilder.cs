namespace TestGenerator;

class CSharpTestSourceBuilder : TestSourceBuilder
{
    public override void WritePartialRenderMethod(string methodName)
    {
        WriteLine($"private static AlphaSkiaImage? {ToPascalCase(methodName)}(AlphaSkiaCanvas canvas)");
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
        WriteLine($"canvas.{ToPascalCase(property)} = {value};");
    }
}