using System.Runtime.InteropServices;
using HarfBuzzSharp;
using SkiaSharp;
using Font = AlphaTab.Model.Font;

namespace TestGenerator;

class SkiaSharpTextMeasurer
{
    // this measurer is inspired on how alphaTab did the text measuring with using
    // SkiaSharp. we know historically from the AlphaTab tests that 
    // this workflow measures text as we want.

    private static readonly IDictionary<string, SKTypeface> CustomTypeFaces =
        new Dictionary<string, SKTypeface>(StringComparer.OrdinalIgnoreCase);

    public static void RegisterCustomFont(byte[] data)
    {
        using var skData = SKData.CreateCopy(data);
        var face = SKTypeface.FromData(skData);
        CustomTypeFaces[CustomTypeFaceKey(face)] = face;
    }

    public static void RegisterCustomFont(string filePath)
    {
        RegisterCustomFont(File.ReadAllBytes(filePath));
    }

    private static string CustomTypeFaceKey(SKTypeface typeface)
    {
        return CustomTypeFaceKey(typeface.FamilyName, typeface.FontWeight > 400,
            typeface.FontSlant == SKFontStyleSlant.Italic);
    }

    private static string CustomTypeFaceKey(string fontFamily, bool isBold, bool isItalic)
    {
        return fontFamily.ToLowerInvariant() + "_" + isBold + "_" + isItalic;
    }

    static SkiaSharpTextMeasurer()
    {
        var repositoryRoot = PathUtils.RepositoryRoot;
        RegisterCustomFont(Path.Combine(repositoryRoot, "lib", "test", "font", "bravura", "Bravura.ttf"));

        RegisterCustomFont(Path.Combine(repositoryRoot, "lib", "test", "font", "roboto", "Roboto-Regular.ttf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "lib", "test", "font", "roboto", "Roboto-Bold.ttf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "lib", "test", "font", "roboto", "Roboto-Italic.ttf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "lib", "test", "font", "roboto", "Roboto-BoldItalic.ttf"));

        RegisterCustomFont(Path.Combine(repositoryRoot, "lib", "test", "font", "ptserif", "PTSerif-Regular.ttf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "lib", "test", "font", "ptserif", "PTSerif-Bold.ttf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "lib", "test", "font", "ptserif", "PTSerif-Italic.ttf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "lib", "test", "font", "ptserif", "PTSerif-BoldItalic.ttf"));
    }

   
    public static double MeasureText(Font font, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var typeFace = GetTypeFace(font);
        using var harfBuzzFont = MakeHarfBuzzFont(typeFace, (int)font.Size);
        using var buffer = new HarfBuzzSharp.Buffer
        {
            Direction = Direction.LeftToRight,
            Language = Language.Default
        };
        buffer.AddUtf8(text);
        harfBuzzFont.Shape(buffer);

        var infos = buffer.GlyphInfos;
        var positions = buffer.GlyphPositions;
        var width = 0.0f;
        for (var i = 0; i < infos.Length; i++)
        {
            width += HarfBuzzToSkiaFontSize * positions[i].XAdvance;
        }

        return width;
    }


    private const int SkiaToHarfBuzzFontSize = 1 << 16;
    private const float HarfBuzzToSkiaFontSize = 1f / SkiaToHarfBuzzFontSize;

    private static HarfBuzzSharp.Font MakeHarfBuzzFont(SKTypeface typeface, int size)
    {
        using var stream = typeface.OpenStream(out var ttcIndex);
        var data = Marshal.AllocCoTaskMem(stream.Length);
        stream.Read(data, stream.Length);
        using var blob = new Blob(data, stream.Length, MemoryMode.ReadOnly,
            () => { Marshal.FreeCoTaskMem(data); });
        blob.MakeImmutable();

        using var face = new Face(blob, ttcIndex)
        {
            Index = ttcIndex,
            UnitsPerEm = typeface.UnitsPerEm
        };

        var font = new HarfBuzzSharp.Font(face);
        var scale = size * SkiaToHarfBuzzFontSize;
        font.SetScale(scale, scale);
        font.SetFunctionsOpenType();
        return font;
    }

    private static SKTypeface GetTypeFace(Font font)
    {
        var key = CustomTypeFaceKey(font.Family, font.IsBold, font.IsItalic);
        if (!CustomTypeFaces.TryGetValue(key, out var typeFace))
        {
            typeFace = SKTypeface.FromFamilyName(font.Family,
                font.IsBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                font.IsItalic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright
            );
        }

        return typeFace;
    }
}