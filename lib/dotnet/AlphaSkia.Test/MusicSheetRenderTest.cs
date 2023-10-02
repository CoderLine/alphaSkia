namespace AlphaSkia.Test;

public abstract class MusicSheetRenderTest
{
    protected static AlphaSkiaTypeface MusicTypeface { get; private set; } = null!;
    protected static float RenderScale { get; private set; } = 1;
    protected static int MusicFontSize { get; private set; } = 34;

    private static readonly IDictionary<string, AlphaSkiaTypeface> CustomTypefaces =
        new Dictionary<string, AlphaSkiaTypeface>(StringComparer.OrdinalIgnoreCase);


    private static string CustomTypefaceKey(string fontFamily, bool isBold, bool isItalic)
    {
        return fontFamily.ToLowerInvariant() + "_" + isBold + "_" + isItalic;
    }

    [OneTimeSetUp]
    public static void Setup()
    {
        AlphaSkiaPlatform.LoadLibrary();

        var bravura = ReadFont("font/bravura/Bravura.ttf");

        MusicTypeface = AlphaSkiaTypeface.Register(bravura) ??
                        throw new InvalidOperationException("Could not load bravura font");

        CustomTypefaces[CustomTypefaceKey("Roboto", false, false)] =
            AlphaSkiaTypeface.Register(ReadFont("font/roboto/Roboto-Regular.ttf")) ??
            throw new InvalidOperationException("Could not load font");
        CustomTypefaces[CustomTypefaceKey("Roboto", true, false)] =
            AlphaSkiaTypeface.Register(ReadFont("font/roboto/Roboto-Bold.ttf")) ??
            throw new InvalidOperationException("Could not load font");
        CustomTypefaces[CustomTypefaceKey("Roboto", false, true)] =
            AlphaSkiaTypeface.Register(ReadFont("font/roboto/Roboto-Italic.ttf")) ??
            throw new InvalidOperationException("Could not load font");
        CustomTypefaces[CustomTypefaceKey("Roboto", true, true)] =
            AlphaSkiaTypeface.Register(ReadFont("font/roboto/Roboto-BoldItalic.ttf")) ??
            throw new InvalidOperationException("Could not load font");

        CustomTypefaces[CustomTypefaceKey("PT Serif", false, false)] =
            AlphaSkiaTypeface.Register(ReadFont("font/ptserif/PTSerif-Regular.ttf")) ??
            throw new InvalidOperationException("Could not load font");
        CustomTypefaces[CustomTypefaceKey("PT Serif", true, false)] =
            AlphaSkiaTypeface.Register(ReadFont("font/ptserif/PTSerif-Bold.ttf")) ??
            throw new InvalidOperationException("Could not load font");
        CustomTypefaces[CustomTypefaceKey("PT Serif", false, true)] =
            AlphaSkiaTypeface.Register(ReadFont("font/ptserif/PTSerif-Italic.ttf")) ??
            throw new InvalidOperationException("Could not load font");
        CustomTypefaces[CustomTypefaceKey("PT Serif", true, true)] =
            AlphaSkiaTypeface.Register(ReadFont("font/ptserif/PTSerif-BoldItalic.ttf")) ??
            throw new InvalidOperationException("Could not load font");
    }

    protected static AlphaSkiaTypeface GetTypeface(string name, bool isBold, bool isItalic)
    {
        if (!CustomTypefaces.TryGetValue(CustomTypefaceKey(name, isBold, isItalic), out var face))
        {
            throw new InvalidOperationException("Unknown font requested: " + name);
        }

        return face;
    }


    protected AlphaSkiaTextAlign TextAlign { get; set; } = AlphaSkiaTextAlign.Center;
    protected AlphaSkiaTextBaseline TextBaseline { get; set; } = AlphaSkiaTextBaseline.Top;
    protected AlphaSkiaTypeface Typeface { get; set; } = null!;
    protected float FontSize { get; set; }

    private static byte[] ReadFont(string relativePath)
    {
        var resourceName = "AlphaSkia.Test." + relativePath.Replace("/", ".");
        using var stream = typeof(MusicSheetRenderTest).Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new IOException("Could not find resource " + relativePath);
        }

        var data = new byte[stream.Length];
        stream.ReadExactly(data);
        return data;
    }

    private AlphaSkiaImage RenderFullImage()
    {
        using var canvas = new AlphaSkiaCanvas();

        var images = AllParts.Select(p => p(canvas)).ToArray();

        canvas.BeginRender(TotalWidth, TotalHeight, RenderScale);

        for (var i = 0; i < images.Length; i++)
        {
            canvas.DrawImage(images[i], PartPositions[i, 0], PartPositions[i, 1], images[i].Width / RenderScale, images[i].Height / RenderScale);
            images[i].Dispose();
        }

        return canvas.EndRender()!;
    }

    [Test]
    public void RenderTest()
    {
        using var finalImage = RenderFullImage();
        var fileName = GetType().Name;
        File.WriteAllBytes(Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName + ".png"),
            finalImage.ToPng()!);
    }

    protected abstract int TotalWidth { get; }
    protected abstract int TotalHeight { get; }
    protected abstract float[,] PartPositions { get; }

    protected abstract Func<AlphaSkiaCanvas, AlphaSkiaImage>[] AllParts { get; }
}