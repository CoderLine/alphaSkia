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
            if ((int)_color.Raw == (int)value.Raw)
            {
                return;
            }
            _source.WriteSetCanvasProperty("Color",
                _source.MakeCallStaticMethod("AlphaSkiaCanvas", "RgbaToColor",
                    $"{_source.MakeCastToByte(value.R)}, {_source.MakeCastToByte(value.G)}, {_source.MakeCastToByte(value.B)}, {_source.MakeCastToByte(value.A)}",
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
            _source.WriteSetCanvasProperty("LineWidth", $"{_source.MakeCastToFloat(value)}");
            _lineWidth = value;
        }
    }

    public Font Font
    {
        get => _font;
        set
        {
            _source.WriteSetProperty("Typeface", _source.MakeCallMethod(_source.MakeMethodAccess("GetTypeface"),
                $"\"{value.Families[0]}\", {(value.IsBold ? "true" : "false")}, {(value.IsItalic ? "true" : "false")}",
                false
            ));
            _source.WriteSetProperty("FontSize", $"{_source.MakeCastToFloat(value.Size)}");
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
            _source.WriteSetProperty("TextBaseline", _source.MakeEnumAccess("AlphaSkiaTextBaseline", value.ToString()));
            _textBaseline = value;
        }
    }

    public void FillRect(double x, double y, double w, double h)
    {
        _source.WriteCallCanvasMethod("FillRect", $"{_source.MakeCastToFloat(x)}, {_source.MakeCastToFloat(y)}, {_source.MakeCastToFloat(w)}, {_source.MakeCastToFloat(h)}");
    }

    public void StrokeRect(double x, double y, double w, double h)
    {
        _source.WriteCallCanvasMethod("StrokeRect", $"{_source.MakeCastToFloat(x)}, {_source.MakeCastToFloat(y)}, {_source.MakeCastToFloat(w)}, {_source.MakeCastToFloat(h)}");
    }

    public void FillCircle(double x, double y, double radius)
    {
        _source.WriteCallCanvasMethod("FillCircle", $"{_source.MakeCastToFloat(x)}, {_source.MakeCastToFloat(y)}, {_source.MakeCastToFloat(radius)}");
    }

    public void StrokeCircle(double x, double y, double radius)
    {
        _source.WriteCallCanvasMethod("StrokeCircle", $"{_source.MakeCastToFloat(x)}, {_source.MakeCastToFloat(y)}, {_source.MakeCastToFloat(radius)}");
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
            $"{_source.EncodeString(text)}, {_source.MakeGetProperty("Typeface")}, {_source.MakeGetProperty("FontSize")}, {_source.MakeCastToFloat(x)}, {_source.MakeCastToFloat(y)}, {_source.MakeGetProperty("TextAlign")}, {_source.MakeGetProperty("TextBaseline")}");
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
        var text = _source.EncodeString(
            new string(symbols.Select(s => (char)((int)s)).ToArray()));

        var textAlign = centerAtPosition
            ? _source.MakeEnumAccess("AlphaSkiaTextAlign", "Center")
            : _source.MakeEnumAccess("AlphaSkiaTextAlign", "Left");
        
        _source.WriteCallCanvasMethod("FillText",
            $"{text}, {_source.MakeGetProperty("MusicTypeface")}, {MakeGetMusicFontSize(scale)}, {_source.MakeCastToFloat(x)}, {_source.MakeCastToFloat(y)}, {textAlign}, {_source.MakeEnumAccess("AlphaSkiaTextBaseline", "Alphabetic")}");
    }

    private string MakeGetMusicFontSize(double? scale = null) 
    {
        if(scale == null)
        {
            return _source.MakeCastToFloat(_source.MakeGetProperty("MusicFontSize"));
        }
        else
        {
            return _source.MakeCastToFloat($"{_source.MakeGetProperty("MusicFontSize")} * {scale}");
        }
    }

    public void BeginRender(double width, double height)
    {
        _source.Resume();
        var methodName = "DrawMusicSheetPart" + _partialCounter++;
        _source.WriteMethodDeclaration("private", "AlphaSkiaImage", methodName, new[] { ("AlphaSkiaCanvas", "canvas" ) });
        _source.BeginBlock();
        _source.WriteCallCanvasMethod("BeginRender", $"{(int)width}, {(int)height}, {_source.MakeCastToFloat(_source.MakeGetProperty("RenderScale"))}");
    }

    public object? EndRender()
    {
        _source.WriteLine(
            $"return {_source.MakeCallMethod(_source.MakeMethodAccess("canvas", "EndRender"), notNull: true)}");
        _source.EndBlock();
        _source.Suspend();
        return _source;
    }

    public object? OnRenderFinished()
    {
        return null;
    }

    public void BeginRotate(double centerX, double centerY, double angle)
    {
        _source.WriteCallCanvasMethod("BeginRotate", $"{_source.MakeCastToFloat(centerX)}, {_source.MakeCastToFloat(centerY)}, {_source.MakeCastToFloat(angle)}");
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
        _source.WriteCallCanvasMethod("MoveTo", $"{_source.MakeCastToFloat(x)}, {_source.MakeCastToFloat(y)}");
    }

    public void LineTo(double x, double y)
    {
        _source.WriteCallCanvasMethod("LineTo", $"{_source.MakeCastToFloat(x)}, {_source.MakeCastToFloat(y)}");
    }

    public void BezierCurveTo(double cp1X, double cp1Y, double cp2X, double cp2Y, double x, double y)
    {
        _source.WriteCallCanvasMethod("BezierCurveTo",
            $"{_source.MakeCastToFloat(cp1X)}, {_source.MakeCastToFloat(cp1Y)}, {_source.MakeCastToFloat(cp2X)}, {_source.MakeCastToFloat(cp2Y)}, {_source.MakeCastToFloat(x)}, {_source.MakeCastToFloat(y)}");
    }

    public void QuadraticCurveTo(double cpx, double cpy, double x, double y)
    {
        _source.WriteCallCanvasMethod("QuadraticCurveTo", $"{_source.MakeCastToFloat(cpx)}, {_source.MakeCastToFloat(cpy)}, {_source.MakeCastToFloat(x)}, {_source.MakeCastToFloat(y)}");
    }
}