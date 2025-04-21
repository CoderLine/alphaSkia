using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace TestGenerator;

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
        return $"u{base.EncodeString(text)}";
    }
    protected override bool SupportsUtf32EscapeSequence => true;

    public override string MakeStringArray(IList<string> values)
    {
        var items = string.Join(", ", values.Select(v => base.EncodeString(v)));
        return "{" + items + "}";
    }
}