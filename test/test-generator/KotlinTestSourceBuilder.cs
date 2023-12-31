namespace TestGenerator;

class KotlinTestSourceBuilder : TestSourceBuilder
{
    public override void WritePartialRenderMethod(string methodName)
    {
        Write($"private fun {ToCamelCase(methodName)}(canvas: AlphaSkiaCanvas): AlphaSkiaImage?");
    }

    public override string MakeCallTestMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"{ToCamelCase(methodName)}", args);
    }

    public override string MakeCallHelperMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"AlphaSkiaCanvas.{ToCamelCase(methodName)}", args);
    }

    public override string MakeCallCanvasMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"canvas.{ToCamelCase(methodName)}", args);
    }

    public override string MakeCastToFloat(string value)
    {
        return $"{value}.toFloat()";
    }

    public override string MakeCastToByte(double value)
    {
        return $"{value}.toByte()";
    }

    private string MakeCallMethod(string methodName, string args = "",
        bool notNull = false)
    {
        return $"{methodName}({args}){(notNull ? "!" : "")}";
    }


    public override void WriteSetTestProperty(string property, string value)
    {
        WriteLine($"{ToCamelCase(property)} = {value}");
    }

    public override string MakeTestGetProperty(string property)
    {
        return $"{ToCamelCase(property)}";
    }

    public override string MakeEnumAccess(string type, string field)
    {
        return $"{type}.{ToSnakeCase(field).ToUpperInvariant()}";
    }

    public override void WriteSetCanvasProperty(string property, string value)
    {
        WriteLine($"canvas.{ToCamelCase(property)} = {value}");
    }
}