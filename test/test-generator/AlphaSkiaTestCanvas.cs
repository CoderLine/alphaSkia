using AlphaTab;
using AlphaTab.Model;
using AlphaTab.Platform;

namespace TestGenerator;

class AlphaSkiaTestCanvas : ICanvas
{
    private readonly TestSourceBuilder _testSource;
    private Color _color = new(255, 255, 255);
    private double _lineWidth = 1;
    private Font _font = new("Arial", 10);
    private TextAlign _textAlign = TextAlign.Left;
    private TextBaseline _textBaseline = TextBaseline.Top;

    private int _partialCounter = 1;

    public bool WithTextLength { get; set; }

    public AlphaSkiaTestCanvas(TestSourceBuilder testSource)
    {
        _testSource = testSource;
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
            _testSource.WriteSetCanvasProperty("Color",
                _testSource.MakeCallHelperMethod("RgbaToColor",
                    $"{_testSource.MakeCastToByte(value.R)}, {_testSource.MakeCastToByte(value.G)}, {_testSource.MakeCastToByte(value.B)}, {_testSource.MakeCastToByte(value.A)}",
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
            _testSource.WriteSetCanvasProperty("LineWidth", $"{_testSource.MakeCastToFloat(value)}");
            _lineWidth = value;
        }
    }

    public Font Font
    {
        get => _font;
        set
        {
            _testSource.WriteSetTestProperty("Typeface", _testSource.MakeCallTestMethod("GetTypeface",
                $"\"{value.Families[0]}\", {(value.IsBold ? "true" : "false")}, {(value.IsItalic ? "true" : "false")}",
                false
            ));
            _testSource.WriteSetTestProperty("FontSize", $"{_testSource.MakeCastToFloat(value.Size)}");
            _font = value;
        }
    }

    public TextAlign TextAlign
    {
        get => _textAlign;
        set
        {
            _testSource.WriteSetTestProperty("TextAlign", _testSource.MakeEnumAccess("AlphaSkiaTextAlign", value.ToString()));
            _textAlign = value;
        }
    }

    public TextBaseline TextBaseline
    {
        get => _textBaseline;
        set
        {
            _testSource.WriteSetTestProperty("TextBaseline", _testSource.MakeEnumAccess("AlphaSkiaTextBaseline", value.ToString()));
            _textBaseline = value;
        }
    }

    public void FillRect(double x, double y, double w, double h)
    {
        _testSource.WriteCallCanvasMethod("FillRect", $"{_testSource.MakeCastToFloat(x)}, {_testSource.MakeCastToFloat(y)}, {_testSource.MakeCastToFloat(w)}, {_testSource.MakeCastToFloat(h)}");
    }

    public void StrokeRect(double x, double y, double w, double h)
    {
        _testSource.WriteCallCanvasMethod("StrokeRect", $"{_testSource.MakeCastToFloat(x)}, {_testSource.MakeCastToFloat(y)}, {_testSource.MakeCastToFloat(w)}, {_testSource.MakeCastToFloat(h)}");
    }

    public void FillCircle(double x, double y, double radius)
    {
        _testSource.WriteCallCanvasMethod("FillCircle", $"{_testSource.MakeCastToFloat(x)}, {_testSource.MakeCastToFloat(y)}, {_testSource.MakeCastToFloat(radius)}");
    }

    public void StrokeCircle(double x, double y, double radius)
    {
        _testSource.WriteCallCanvasMethod("StrokeCircle", $"{_testSource.MakeCastToFloat(x)}, {_testSource.MakeCastToFloat(y)}, {_testSource.MakeCastToFloat(radius)}");
    }

    public void BeginGroup(string identifier)
    {
    }

    public void EndGroup()
    {
    }

    public void FillText(string text, double x, double y)
    {
        var textPart = WithTextLength ? $"{_testSource.EncodeString(text)}, {text.Length}" :  $"{_testSource.EncodeString(text)}";
        _testSource.WriteCallCanvasMethod("FillText",
            $"{textPart}, {_testSource.MakeTestGetProperty("Typeface")}, {_testSource.MakeTestGetProperty("FontSize")}, {_testSource.MakeCastToFloat(x)}, {_testSource.MakeCastToFloat(y)}, {_testSource.MakeTestGetProperty("TextAlign")}, {_testSource.MakeTestGetProperty("TextBaseline")}");
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
        var text = _testSource.EncodeString(
            new string(symbols.Select(s => (char)((int)s)).ToArray()));

        var textAlign = centerAtPosition
            ? _testSource.MakeEnumAccess("AlphaSkiaTextAlign", "Center")
            : _testSource.MakeEnumAccess("AlphaSkiaTextAlign", "Left");
        

        var textPart = WithTextLength ? $"{text}, {text.Length}" :  $"{text}";
        _testSource.WriteCallCanvasMethod("FillText",
            $"{textPart}, {_testSource.MakeTestGetProperty("MusicTypeface")}, {MakeGetMusicFontSize(scale)}, {_testSource.MakeCastToFloat(x)}, {_testSource.MakeCastToFloat(y)}, {textAlign}, {_testSource.MakeEnumAccess("AlphaSkiaTextBaseline", "Alphabetic")}");
    }

    private string MakeGetMusicFontSize(double? scale = null) 
    {
        if(scale == null)
        {
            return _testSource.MakeCastToFloat(_testSource.MakeTestGetProperty("MusicFontSize"));
        }
        else
        {
            return _testSource.MakeCastToFloat($"{_testSource.MakeTestGetProperty("MusicFontSize")} * {scale}");
        }
    }

    public void BeginRender(double width, double height)
    {
        _testSource.Resume();
        _testSource.WritePartialRenderMethod("DrawMusicSheetPart" + _partialCounter++);
        _testSource.BeginBlock();
        _testSource.WriteCallCanvasMethod("BeginRender", $"{(int)width}, {(int)height}, {_testSource.MakeCastToFloat(_testSource.MakeTestGetProperty("RenderScale"))}");
    }

    public object EndRender()
    {
        _testSource.WriteLine($"return {_testSource.MakeCallCanvasMethod("EndRender", "", true)}");
        _testSource.EndBlock();
        _testSource.Suspend();
        return _testSource;
    }

    public object? OnRenderFinished()
    {
        return null;
    }

    public void BeginRotate(double centerX, double centerY, double angle)
    {
        _testSource.WriteCallCanvasMethod("BeginRotate", $"{_testSource.MakeCastToFloat(centerX)}, {_testSource.MakeCastToFloat(centerY)}, {_testSource.MakeCastToFloat(angle)}");
    }

    public void EndRotate()
    {
        _testSource.WriteCallCanvasMethod("EndRotate");
    }

    public void BeginPath()
    {
        _testSource.WriteCallCanvasMethod("BeginPath");
    }

    public void ClosePath()
    {
        _testSource.WriteCallCanvasMethod("ClosePath");
    }

    public void Fill()
    {
        _testSource.WriteCallCanvasMethod("Fill");
    }

    public void Stroke()
    {
        _testSource.WriteCallCanvasMethod("Stroke");
    }

    public void MoveTo(double x, double y)
    {
        _testSource.WriteCallCanvasMethod("MoveTo", $"{_testSource.MakeCastToFloat(x)}, {_testSource.MakeCastToFloat(y)}");
    }

    public void LineTo(double x, double y)
    {
        _testSource.WriteCallCanvasMethod("LineTo", $"{_testSource.MakeCastToFloat(x)}, {_testSource.MakeCastToFloat(y)}");
    }

    public void BezierCurveTo(double cp1X, double cp1Y, double cp2X, double cp2Y, double x, double y)
    {
        _testSource.WriteCallCanvasMethod("BezierCurveTo",
            $"{_testSource.MakeCastToFloat(cp1X)}, {_testSource.MakeCastToFloat(cp1Y)}, {_testSource.MakeCastToFloat(cp2X)}, {_testSource.MakeCastToFloat(cp2Y)}, {_testSource.MakeCastToFloat(x)}, {_testSource.MakeCastToFloat(y)}");
    }

    public void QuadraticCurveTo(double cpx, double cpy, double x, double y)
    {
        _testSource.WriteCallCanvasMethod("QuadraticCurveTo", $"{_testSource.MakeCastToFloat(cpx)}, {_testSource.MakeCastToFloat(cpy)}, {_testSource.MakeCastToFloat(x)}, {_testSource.MakeCastToFloat(y)}");
    }
}