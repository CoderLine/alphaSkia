namespace AlphaSkia.Test;

/// <summary>
/// Invalid input must be rejected with null or a .NET exception instead of crashing the process.
/// </summary>
internal sealed class RobustnessTest
{
    private int _checks;
    private int _failures;

    public static bool Run()
    {
        return new RobustnessTest().RunChecks();
    }

    private void Check(bool condition, string message)
    {
        _checks++;
        if (!condition)
        {
            _failures++;
            Console.Error.WriteLine($"  FAILED: {message}");
        }
    }

    private void Throws<TException>(Action action, string message) where TException : Exception
    {
        try
        {
            action();
            Check(false, $"{message}: no exception");
        }
        catch (Exception e)
        {
            Check(e is TException, $"{message}: {e.GetType().Name} {e.Message}");
        }
    }

    private bool RunChecks()
    {
        Console.WriteLine("BEGIN: robustness test");
        var garbage = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 0xff, 0xfe };

        Check(AlphaSkiaTypeface.Register(garbage) == null, "typeface from garbage bytes");
        Check(AlphaSkiaImage.Decode(garbage) == null, "decode garbage bytes");
        Check(AlphaSkiaImage.FromPixels(1000, 1000, new byte[16]) == null, "pixels larger than the buffer");
        Check(AlphaSkiaImage.FromPixels(-1, 1, new byte[16]) == null, "pixels with negative width");
        Throws<ArgumentNullException>(() => AlphaSkiaTypeface.Create(null!, 400, false), "typeface without name");
        Throws<ArgumentException>(() => new AlphaSkiaTextStyle(new string[256], 400, false), "256 font families");
        Throws<ArgumentNullException>(() => new AlphaSkiaTextStyle(new string[] { null! }, 400, false), "null font family");

        using var style = new AlphaSkiaTextStyle(new[] { "Noto Sans" }, 400, false);
        using var canvas = new AlphaSkiaCanvas();
        Throws<ArgumentException>(() => canvas.BeginRender(10, 10, 0f), "render scale 0");
        Throws<ArgumentException>(() => canvas.BeginRender(10, 10, float.NaN), "render scale NaN");
        canvas.BeginRender(10, 10);

        Throws<ArgumentNullException>(() => canvas.FillText(null!, style, 12, 0, 0, AlphaSkiaTextAlign.Left, AlphaSkiaTextBaseline.Alphabetic), "fill null text");
        Throws<ArgumentException>(() => canvas.MeasureText("a", style, float.PositiveInfinity, AlphaSkiaTextAlign.Left, AlphaSkiaTextBaseline.Alphabetic), "measure infinite font size");
        Throws<ArgumentNullException>(() => canvas.DrawImage(null!, 0, 0, 1, 1), "draw null image");

        var image = AlphaSkiaImage.FromPixels(1, 1, new byte[4])!;
        image.Dispose();
        image.Dispose();
        Throws<ObjectDisposedException>(() => canvas.DrawImage(image, 0, 0, 1, 1), "draw disposed image");
        Throws<ObjectDisposedException>(() => _ = image.Width, "width of disposed image");

        var metrics = canvas.MeasureText("a", style, 12, AlphaSkiaTextAlign.Left, AlphaSkiaTextBaseline.Alphabetic);
        Check(Math.Abs(metrics.AlphabeticBaseline) < 0.001f, $"alphabetic baseline is {metrics.AlphabeticBaseline}");
        metrics.Dispose();
        Throws<ObjectDisposedException>(() => _ = metrics.Width, "width of disposed metrics");

        var disposedStyle = new AlphaSkiaTextStyle(new[] { "Noto Sans" }, 400, false);
        disposedStyle.Dispose();
        Throws<ObjectDisposedException>(() => canvas.FillText("a", disposedStyle, 12, 0, 0, AlphaSkiaTextAlign.Left, AlphaSkiaTextBaseline.Alphabetic), "fill text with disposed style");

        using (var rendered = canvas.EndRender())
        {
            Check(rendered != null, "end render");
        }

        Console.WriteLine($"  {_checks - _failures}/{_checks} checks passed");
        if (_failures > 0)
        {
            Console.WriteLine("FAILED: robustness test");
            return false;
        }

        Console.WriteLine("SUCCESS: robustness test");
        return true;
    }
}
