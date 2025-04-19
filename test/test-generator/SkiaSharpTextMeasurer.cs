using System.Runtime.InteropServices;
using AlphaTab.Platform;
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
        RegisterCustomFont(Path.Combine(repositoryRoot, "test", "test-data", "font", "bravura", "Bravura.otf"));

        RegisterCustomFont(Path.Combine(repositoryRoot, "test", "test-data", "font", "noto-sans", "NotoSans-Regular.otf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "test", "test-data", "font", "noto-sans", "NotoSans-Bold.otf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "test", "test-data", "font", "noto-sans", "NotoSans-Italic.otf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "test", "test-data", "font", "noto-sans", "NotoSans-BoldItalic.otf"));

        RegisterCustomFont(Path.Combine(repositoryRoot, "test", "test-data", "font", "noto-serif", "NotoSerif-Regular.otf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "test", "test-data", "font", "noto-serif", "NotoSerif-Bold.otf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "test", "test-data", "font", "noto-serif", "NotoSerif-Italic.otf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "test", "test-data", "font", "noto-serif", "NotoSerif-BoldItalic.otf"));
            
        RegisterCustomFont(Path.Combine(repositoryRoot, "test", "test-data", "font", "noto-music", "NotoMusic-Regular.otf"));
        RegisterCustomFont(Path.Combine(repositoryRoot, "test", "test-data", "font", "noto-color-emoji", "NotoColorEmoji-Regular.ttf"));
    }

    public static MeasuredText MeasureText(Font font, string text)
    {
        // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/modules/canvas/canvas2d/base_rendering_context_2d.cc;l=1288;bpv=0;bpt=1

        if (string.IsNullOrEmpty(text))
        {
            return new MeasuredText(0, 0);
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

        // https://source.chromium.org/chromium/chromium/src/+/refs/tags/120.0.6099.81:third_party/blink/renderer/platform/fonts/font.cc;l=387;bpv=0;bpt=0

        var glyphBoundsY = 0.0f;
        var glyphBoundsBottom = 0.0f;

        using var fontData = new SKFont(typeFace, (float)font.Size);
        fontData.GetFontMetrics(out var fontMetrics);
        
        var x = 0;
        var width = 0;
        var y = 0;

        for (var i = 0; i < infos.Length; i++)
        {
            var yOffset = y - positions[i].YOffset;

            var glyphBottom = yOffset;
            if (harfBuzzFont.TryGetGlyphExtents(infos[i].Codepoint, out var glyphExtents))
            {
                yOffset += glyphExtents.YBearing;
                glyphBottom += glyphExtents.YBearing + glyphExtents.Height;
            }

            if (glyphBoundsY < yOffset)
            {
                glyphBoundsY = yOffset;
            }

            if (glyphBottom > glyphBoundsBottom)
            {
                glyphBoundsBottom = glyphBottom;
            }

            x += positions[i].XAdvance;
            if (x > width)
            {
                width = x;
            }
            y += positions[i].YAdvance;
        }

        // default text-baseline in canvas is alphabetic. there baseline is 0
        // https://github.com/chromium/chromium/blob/99314be8152e688bafbbf9a615536bdbb289ea87/third_party/blink/renderer/core/html/canvas/text_metrics.cc#L34
        var baselineY = 0;

        var actualBoundingBoxAscent = -glyphBoundsY - baselineY;
        var actualBoundingBoxDescent = glyphBoundsBottom + baselineY;

        var height = actualBoundingBoxDescent - actualBoundingBoxAscent;

        return new MeasuredText((double)width / SkiaToHarfBuzzFontSize, height / SkiaToHarfBuzzFontSize);
    }

    private const int SkiaToHarfBuzzFontSize = 1 << 16;

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