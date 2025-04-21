namespace AlphaSkia.Test;

partial class AlphaTabGeneratedRenderTest
{
    public static float RenderScale { get; set; } = 1;
    public static AlphaSkiaTextStyle MusicTextStyle { get; set; } = null!;
    public static readonly float MusicFontSize = 34;
    public static AlphaSkiaTextAlign TextAlign { get; set; } = AlphaSkiaTextAlign.Left;
    public static AlphaSkiaTextBaseline TextBaseline { get; set; } = AlphaSkiaTextBaseline.Top;
    public static AlphaSkiaTextStyle TextStyle { get; set; } = null!;
    public static float FontSize { get; set; } = 12;

    private static readonly IDictionary<string, AlphaSkiaTextStyle> CustomTextStyles =
        new Dictionary<string, AlphaSkiaTextStyle>(StringComparer.OrdinalIgnoreCase);

    private static string CustomTypefaceKey(string[] fontFamily, int weight, bool isItalic)
    {
        return string.Join("_", fontFamily).ToLowerInvariant() + "_" + weight + "_" + isItalic;
    }

    private static AlphaSkiaTextStyle GetTextStyle(string[] fontFamily, int weight, bool isItalic)
    {
        var key = CustomTypefaceKey(fontFamily, weight, isItalic);
        if (!CustomTextStyles.TryGetValue(key, out var textStyle))
        {
            textStyle = new AlphaSkiaTextStyle(fontFamily, (ushort)weight, isItalic);
            CustomTextStyles[key] = textStyle;
        }

        return textStyle;
    }

    public static AlphaSkiaTypeface LoadTypeface(string filePath)
    {
        Console.WriteLine($"Loading typeface from {filePath}");
        var data = File.ReadAllBytes(filePath);

        Console.WriteLine($"Read {data.Length} bytes from file, decoding typeface");
        var typeface = AlphaSkiaTypeface.Register(data);

        return typeface
               ?? throw new InvalidOperationException("Could not create typeface from data");
    }
}