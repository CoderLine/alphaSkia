namespace TestGenerator;

class TypeScriptTestSourceBuilder : TestSourceBuilder
{
    public override void WritePartialRenderMethod(string methodName)
    {
        Write($"function {ToCamelCase(methodName)}(canvas: AlphaSkiaCanvas): AlphaSkiaImage ");
    }

    public override string MakeCallTestMethod(string methodName, string args, bool statement)
    {
        return MakeCallMethod($"TestBase.{ToCamelCase(methodName)}", args, statement);
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
        return value;
    }

    public override string MakeCastToByte(double value)
    {
        return ((int)value).ToString();
    }
    
     
    private string MakeCallMethod(string methodName, string args = "", bool statement = true,
        bool notNull = false)
    {
        return $"{methodName}({args}){(notNull ? "!" : "")}{(statement ? ";" : "")}";
    }
    
    public override void WriteSetTestProperty(string property, string value)
    {
        WriteLine($"TestBase.{ToCamelCase(property)} = {value};");
    }

    public override string MakeTestGetProperty(string property)
    {
        return $"TestBase.{ToCamelCase(property)}";
    }

    public override string MakeEnumAccess(string type, string field)
    {
        return $"{type}.{ToPascalCase(field)}";
    }

    public override void WriteSetCanvasProperty(string property, string value)
    {
        WriteLine($"canvas.{ToCamelCase(property)} = {value};");
    }
}