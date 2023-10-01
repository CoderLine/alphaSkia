using AlphaTab;
using AlphaTab.Model;
using AlphaTab.Platform;

namespace TestGenerator;

class AlphaSkiaTestCanvas : ICanvas
{
    private readonly SourceBuilder _source;
    private Color _color = new Color(255, 255, 255, 0xFF);
    private double _lineWidth = 1;
    private Font _font = new Font("Arial", 10);
    private TextAlign _textAlign = TextAlign.Left;
    private TextBaseline _textBaseline = TextBaseline.Top;

    private int _partialCounter = 1;

    public AlphaSkiaTestCanvas(SourceBuilder source)
    {
        _source = source;
    }

    public Settings Settings { get; set; } = new();

    public Color Color
    {
        get => _color;
        set
        {
            _source.WriteSetCanvasProperty("Color",
                _source.MakeCallStaticMethod("AlphaSkiaCanvas", "RgbaToColor",
                    $"(byte){(byte)value.R}, (byte){(byte)value.G}, (byte){(byte)value.B}, (byte){(byte)value.A}, ",
                    false
                )
            );
            _color = value;
        }
    }

    public double LineWidth
    {
        get => _lineWidth;
        set
        {
            _source.WriteSetCanvasProperty("LineWidth", $"(float){value}");
            _lineWidth = value;
        }
    }

    public Font Font
    {
        get => _font;
        set
        {
            _source.WriteSetProperty("Typeface", _source.MakeCallMethod("GetTypeface",
                $"\"{value.Families[0]}\", {(value.IsBold ? "true" : "false")}, {(value.IsItalic ? "true" : "false")}",
                false
            ));
            _source.WriteSetProperty("FontSize", $"(float){value.Size}");
            _font = value;
        }
    }

    public TextAlign TextAlign
    {
        get => _textAlign;
        set
        {
            _source.WriteSetProperty("TextAlign", _source.MakeEnumAccess("AlphaSkiaTextAlign", value.ToString()));
            _textAlign = value;
        }
    }

    public TextBaseline TextBaseline
    {
        get => _textBaseline;
        set
        {
            _source.WriteSetProperty("TextBaseline", _source.MakeEnumAccess("TextBaseline", value.ToString()));
            _textBaseline = value;
        }
    }

    public void FillRect(double x, double y, double w, double h)
    {
        _source.WriteCallCanvasMethod("FillRect", $"(float){x}, (float){y}, (float){w}, (float){h}");
    }

    public void StrokeRect(double x, double y, double w, double h)
    {
        _source.WriteCallCanvasMethod("StrokeRect", $"(float){x}, (float){y}, (float){w}, (float){h}");
    }

    public void FillCircle(double x, double y, double radius)
    {
        _source.WriteCallCanvasMethod("FillCircle", $"(float){x}, (float){y}, (float){radius}");
    }

    public void StrokeCircle(double x, double y, double radius)
    {
        _source.WriteCallCanvasMethod("StrokeCircle", $"(float){x}, (float){y}, (float){radius}");
    }

    public void BeginGroup(string identifier)
    {
    }

    public void EndGroup()
    {
    }

    public void FillText(string text, double x, double y)
    {
        _source.WriteCallCanvasMethod("FillText",
            $"{_source.EncodeString(text)}, {_source.MakeGetProperty("Typeface")}, {_source.MakeGetProperty("FontSize")}, (float){x}, (float){y}, {_source.MakeGetProperty("TextAlign")}, {_source.MakeGetProperty("TextBaseline")}");
    }

    public double MeasureText(string text)
    {
        return SkiaSharpTextMeasurer.MeasureText(_font, text);
    }

    public void FillMusicFontSymbol(double x, double y, double scale, MusicFontSymbol symbol,
        bool centerAtPosition = false)
    {
        FillMusicFontSymbols(x, y, scale, new List<MusicFontSymbol>
        {
            symbol
        }, centerAtPosition);
    }

    public void FillMusicFontSymbols(double x, double y, double scale, IList<MusicFontSymbol> symbols,
        bool centerAtPosition = false)
    {
        var text = "\"" + string.Join("", symbols.Select(s => _source.UnicodeEscape((int)s))) + '"';

        var textAlign = centerAtPosition
            ? _source.MakeEnumAccess("AlphaSkiaTextAlign", "Center")
            : _source.MakeEnumAccess("AlphaSkiaTextAlign", "Left");
        _source.WriteCallCanvasMethod("FillText",
            $"{text}, {_source.MakeGetProperty("MusicTypeface")}, {_source.MakeGetProperty("MusicFontSize")}, (float){x}, (float){y}, {textAlign}, {_source.MakeEnumAccess("AlphaSkiaTextBaseline", "Alphabetic")}");
    }

    public void BeginRender(double width, double height)
    {
        var methodName = _source.MakeMethodAccess("DrawMusicSheetPart" + _partialCounter++);
        _source.WriteLine($"private AlphaSkiaImage {methodName}(AlphaSkiaCanvas canvas)");
        _source.BeginBlock();
        _source.WriteCallCanvasMethod("BeginRender", $"(int){width}, (int){height}");
    }

    public object? EndRender()
    {
        _source.WriteLine(
            $"return {_source.MakeCallMethod(_source.MakeMethodAccess("canvas", "EndRender"))}");
        _source.EndBlock();
        return _source;
    }

    public object? OnRenderFinished()
    {
        return null;
    }

    public void BeginRotate(double centerX, double centerY, double angle)
    {
        _source.WriteCallCanvasMethod("BeginRotate", $"(float){centerX}, (float){centerY}, (float){angle}");
    }

    public void EndRotate()
    {
        _source.WriteCallCanvasMethod("EndRotate");
    }

    public void BeginPath()
    {
        _source.WriteCallCanvasMethod("BeginPath");
    }

    public void ClosePath()
    {
        _source.WriteCallCanvasMethod("ClosePath");
    }

    public void Fill()
    {
        _source.WriteCallCanvasMethod("Fill");
    }

    public void Stroke()
    {
        _source.WriteCallCanvasMethod("Stroke");
    }

    public void MoveTo(double x, double y)
    {
        _source.WriteCallCanvasMethod("MoveTo", $"(float){x}, (float){y}");
    }

    public void LineTo(double x, double y)
    {
        _source.WriteCallCanvasMethod("LineTo", $"(float){x}, (float){y}");
    }

    public void BezierCurveTo(double cp1X, double cp1Y, double cp2X, double cp2Y, double x, double y)
    {
        _source.WriteCallCanvasMethod("BezierCurveTo",
            $"(float){cp1X}, (float){cp1Y}, (float){cp2X}, (float){cp2Y}, (float){x}, (float){y}");
    }

    public void QuadraticCurveTo(double cpx, double cpy, double x, double y)
    {
        _source.WriteCallCanvasMethod("QuadraticCurveTo", $"(float){cpx}, (float){cpy}, (float){x}, (float){y}");
    }
}