using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace TestGenerator;

abstract partial class TestSourceBuilder
{
    private readonly StringBuilder _source = new();
    private bool _isStartOfLine;
    private int _indent;
    private bool _suspended;

    public override string ToString()
    {
        return _source.ToString();
    }

    public void WriteLine(string? text = null)
    {
        if (_suspended)
        {
            return;
        }

        if (text != null)
        {
            Write(text);
        }

        _source.AppendLine();
        _isStartOfLine = true;
    }

    public void Write(string text)
    {
        if (_suspended)
        {
            return;
        }

        WriteIndent();
        _source.Append(text);
        _isStartOfLine = false;
    }

    private void WriteIndent()
    {
        if (_isStartOfLine && _indent > 0)
        {
            _source.Append("".PadLeft(4 * _indent, ' '));
            _isStartOfLine = false;
        }
    }

    public void BeginBlock()
    {
        if (_suspended)
        {
            return;
        }

        WriteLine("{");
        _indent++;
    }

    public void EndBlock(bool statement = false)
    {
        if (_suspended)
        {
            return;
        }

        _indent--;
        WriteLine("}" + (statement ? ";" : ""));
    }

    public abstract void WritePartialRenderMethod(string methodName);
    public abstract string MakeEnumAccess(string type, string field);

    public abstract string MakeTestGetProperty(string property);

    public abstract string MakeCallHelperMethod(string methodName, string args, bool statement);
    public abstract string MakeCallTestMethod(string methodName, string args, bool statement);
    public abstract string MakeCallCanvasMethod(string methodName, string args, bool statement);

    public abstract void WriteSetTestProperty(string property, string value);
    public abstract void WriteSetCanvasProperty(string property, string value);

    public abstract string MakeCastToFloat(string value);
    public abstract string MakeCastToByte(double value);

    public string MakeCastToFloat(double value)
    {
        return MakeCastToFloat(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public void WriteCallCanvasMethod(string methodName, string args = "", bool statement = true)
    {
        WriteLine(MakeCallCanvasMethod(methodName, args, statement));
    }

    protected static string ToPascalCase(string s)
    {
        return s[..1].ToUpperInvariant() + s[1..];
    }

    protected static string ToCamelCase(string s)
    {
        return s[..1].ToLowerInvariant() + s[1..];
    }

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", RegexOptions.Compiled)]
    private static partial Regex SnakeCaseRegex();

    protected static string ToSnakeCase(string s)
    {
        s = s.Replace("AlphaSkia", "alphaskia");
        return SnakeCaseRegex().Replace(s, "_$1").Trim().ToLower();
    }

    public virtual string EncodeString(string text)
    {
        return JsonSerializer.Serialize(text,
            SupportsUtf32EscapeSequence ? Utf32EscapeSequenceOptions : Utf16EscapeSequenceOptions);
    }

    protected abstract bool SupportsUtf32EscapeSequence { get; }

    private static readonly JsonSerializerOptions Utf32EscapeSequenceOptions = new()
    {
        Encoder = new Utf32EscapeSequenceEncoder()
    };

    // mostly borrowed from DefaultJavaScriptEncoder and OptimizedInboxTextEncoder
    private class Utf32EscapeSequenceEncoder : JavaScriptEncoder
    {
        private readonly HashSet<uint> _allowedBmpCodePoints = [];

        public Utf32EscapeSequenceEncoder()
        {
            var range = UnicodeRanges.BasicLatin;
            var firstCodePoint = range.FirstCodePoint;
            var rangeSize = range.Length;
            for (var i = 0; i < rangeSize; i++)
            {
                var codePoint = firstCodePoint + i;
                _allowedBmpCodePoints.Add((uint)codePoint);
            }
        }

        public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
        {
            var data = new ReadOnlySpan<char>(text, textLength);
            fixed (char* pData = data)
            {
                nuint lengthInChars = (uint)data.Length;

                nuint idx = 0;

                // If there's any leftover data, try consuming it now.

                if (idx < lengthInChars)
                {
                    // unroll the loop 8x
                    nint loopIter;
                    for (; lengthInChars - idx >= 8; idx += 8)
                    {
                        loopIter = -1;
                        if (!_allowedBmpCodePoints.Contains(pData[idx + (nuint)(++loopIter)]))
                        {
                            goto BrokeInUnrolledLoop;
                        }

                        if (!_allowedBmpCodePoints.Contains(pData[idx + (nuint)(++loopIter)]))
                        {
                            goto BrokeInUnrolledLoop;
                        }

                        if (!_allowedBmpCodePoints.Contains(pData[idx + (nuint)(++loopIter)]))
                        {
                            goto BrokeInUnrolledLoop;
                        }

                        if (!_allowedBmpCodePoints.Contains(pData[idx + (nuint)(++loopIter)]))
                        {
                            goto BrokeInUnrolledLoop;
                        }

                        if (!_allowedBmpCodePoints.Contains(pData[idx + (nuint)(++loopIter)]))
                        {
                            goto BrokeInUnrolledLoop;
                        }

                        if (!_allowedBmpCodePoints.Contains(pData[idx + (nuint)(++loopIter)]))
                        {
                            goto BrokeInUnrolledLoop;
                        }

                        if (!_allowedBmpCodePoints.Contains(pData[idx + (nuint)(++loopIter)]))
                        {
                            goto BrokeInUnrolledLoop;
                        }

                        if (!_allowedBmpCodePoints.Contains(pData[idx + (nuint)(++loopIter)]))
                        {
                            goto BrokeInUnrolledLoop;
                        }
                    }

                    for (; idx < lengthInChars; idx++)
                    {
                        if (!_allowedBmpCodePoints.Contains(pData[idx]))
                        {
                            break;
                        }
                    }

                    goto Return;

                    BrokeInUnrolledLoop:
                    idx += (nuint)loopIter;
                }

                Return:

                var idx32 = (int)idx;
                if (idx32 == (int)lengthInChars)
                {
                    idx32 = -1;
                }

                return idx32;
            }
        }

        public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength,
            out int numberOfCharactersWritten)
        {
            var destination = new Span<char>(buffer, bufferLength);

            if (_allowedBmpCodePoints.Contains((uint)unicodeScalar))
            {
                // The bitmap should only allow BMP non-surrogate code points.
                if (!destination.IsEmpty)
                {
                    destination[0] = (char)unicodeScalar; // reflect as-is
                    numberOfCharactersWritten = 1;
                    return true;
                }
            }
            else
            {
                var innerCharsWritten = EncodeUtf16(new Rune(unicodeScalar), destination);
                if (innerCharsWritten >= 0)
                {
                    numberOfCharactersWritten = innerCharsWritten;
                    return true;
                }
            }

            // If we reached this point, we ran out of space in the destination.
            numberOfCharactersWritten = 0;
            return false;
        }

        private int EncodeUtf16(Rune value, Span<char> destination)
        {
            if (value.IsBmp)
            {
                // Write 6 chars: "\uXXXX"
                if (!IsValidIndex(destination, 5))
                {
                    return -1;
                }

                destination[0] = '\\';
                destination[1] = 'u';
                destination[2] = '0';
                destination[3] = '0';
                destination[4] = '0';
                destination[5] = '0';

                var u16 = (ushort)value.Value;
                unsafe
                {
                    var ptr = Unsafe.AsPointer(ref u16);
                    var raw = new Span<byte>(ptr, 2);
                    raw.Reverse();
                    Convert.TryToHexString(raw, destination[2..], out var charsWritten);

                    // pad
                    if (charsWritten < 4)
                    {
                        for (var i = 0; i < charsWritten; i++)
                        {
                            var src = 2 + i;
                            var dst = 5 - i;
                            destination[dst] = destination[src];
                        }
                    }
                }
                
                return 6;
            }

            // "\UXXXXXXXX"
            destination[0] = '\\';
            destination[1] = 'U';
            destination[2] = '0';
            destination[3] = '0';
            destination[4] = '0';
            destination[5] = '0';
            destination[6] = '0';
            destination[7] = '0';
            destination[8] = '0';
            destination[9] = '0';
            var u32 = (uint)value.Value;
            
            unsafe
            {
                var ptr = Unsafe.AsPointer(ref u32);
                var raw = new Span<byte>(ptr, 4);
                raw.Reverse();
                Convert.TryToHexString(raw, destination[2..], out var charsWritten);

                if (charsWritten < 8)
                {
                    for (int i = 0; i < charsWritten; i++)
                    {
                        var src = 2 + i;
                        var dst = 9 - i;
                        destination[dst] = destination[src];
                    }
                }
            }
            return 10;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidIndex<T>(Span<T> span, int index)
        {
            return (uint)index < (uint)span.Length;
        }

        public override bool WillEncode(int unicodeScalar)
        {
            return !_allowedBmpCodePoints.Contains((uint)unicodeScalar);
        }

        public override int MaxOutputCharactersPerInputCharacter => 10; // \UXXXXXXXX
    }

    private static readonly JsonSerializerOptions Utf16EscapeSequenceOptions = new();

    public void Resume()
    {
        _suspended = false;
    }

    public void Suspend()
    {
        _suspended = true;
    }

    public abstract string MakeStringArray(IList<string> values);
}