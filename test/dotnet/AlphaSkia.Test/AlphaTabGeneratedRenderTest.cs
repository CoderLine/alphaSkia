namespace AlphaSkia.Test;

partial class AlphaTabGeneratedRenderTest
{
    public static float RenderScale { get; set; } = 1;
    public static AlphaSkiaTypeface MusicTypeface { get; set; } = null!;
    public static readonly float MusicFontSize = 34;
    public static AlphaSkiaTextAlign TextAlign { get; set; } = AlphaSkiaTextAlign.Left;
    public static AlphaSkiaTextBaseline TextBaseline { get; set; } = AlphaSkiaTextBaseline.Top;
    public static AlphaSkiaTypeface Typeface { get; set; } = null!;
    public static float FontSize { get; set; } = 12;

    private static readonly IDictionary<string, AlphaSkiaTypeface> CustomTypefaces =
        new Dictionary<string, AlphaSkiaTypeface>(StringComparer.OrdinalIgnoreCase);

    private static string CustomTypefaceKey(string fontFamily, bool isBold, bool isItalic)
    {
        return fontFamily.ToLowerInvariant() + "_" + isBold + "_" + isItalic;
    }

    private static AlphaSkiaTypeface GetTypeface(string fontFamily, bool isBold, bool isItalic)
    {
        var key = CustomTypefaceKey(fontFamily, isBold, isItalic);
        if (!CustomTypefaces.TryGetValue(key, out var typeface))
        {
            throw new InvalidOperationException($"Unknown font requested: {key}");
        }

        return typeface;
    }

    public static AlphaSkiaTypeface LoadTypeface(string name, bool isBold, bool isItalic, string filePath)
    {
        var key = CustomTypefaceKey(name, isBold, isItalic);
        Console.WriteLine($"Loading typeface {key} from {filePath}");
        var data = File.ReadAllBytes(filePath);

        Console.WriteLine($"Read {data.Length} bytes from file, decoding typeface");
        var typeface = AlphaSkiaTypeface.Register(data);

        CustomTypefaces[key] = typeface
                               ?? throw new InvalidOperationException("Could not create typeface from data");
        return typeface;
    }
}