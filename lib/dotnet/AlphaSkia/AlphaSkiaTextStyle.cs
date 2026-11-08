using System.Runtime.InteropServices;

namespace AlphaSkia;

/// <summary>
/// Represents a text style which can be used to draw or measure texts 
/// with support for mixed fonts for fallback character rendering.
/// </summary>
/// <remarks>
/// This structure is designed for good reusability. The same text style might be used 
/// with different font sizes, colors and alignments, that's why they are not part of this structure itself.
/// </remarks>
public sealed class AlphaSkiaTextStyle : AlphaSkiaNative
{
    /// <summary>
    /// Gets the list of font family names which are consulted for finding 
    /// typefaces with glyphs for drawing or measuring texts.
    /// </summary>
    public string[] FontFamilies { get; }

    /// <summary>
    /// Gets the font weight used for finding typefaces.
    /// </summary>
    public ushort Weight { get; }

    /// <summary>
    /// Gets whether the used typefaces should be italic. 
    /// </summary>
    public bool IsItalic { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlphaSkiaTextStyle"/> class.
    /// </summary>
    /// <param name="fontFamilies">The list of font family names.</param>
    /// <param name="weight">The font weight typefaces should have.</param>
    /// <param name="isItalic">Whether typefaces should be italic.</param>
    /// <exception cref="ArgumentNullException">The font families or one of them are null.</exception>
    /// <exception cref="ArgumentException">More than 255 font families are given.</exception>
    public AlphaSkiaTextStyle(string[] fontFamilies, ushort weight, bool isItalic)
        : base(CreateTextStyle(fontFamilies, weight, isItalic), NativeMethods.alphaskia_text_style_free)
    {
        FontFamilies = fontFamilies;
        Weight = weight;
        IsItalic = isItalic;
    }

    private static IntPtr CreateTextStyle(string[] fontFamilies, ushort weight, bool isItalic)
    {
        if (fontFamilies == null)
        {
            throw new ArgumentNullException(nameof(fontFamilies));
        }

        if (fontFamilies.Length > byte.MaxValue)
        {
            throw new ArgumentException("At most 255 font families are supported", nameof(fontFamilies));
        }

        var utf8FamilyNames = new IntPtr[fontFamilies.Length];
        try
        {
            for (var i = 0; i < fontFamilies.Length; i++)
            {
                var familyName = fontFamilies[i] ?? throw new ArgumentNullException(nameof(fontFamilies));
                utf8FamilyNames[i] = NativeMethods.AllocUtf8(familyName);
            }

            return NativeMethods.alphaskia_text_style_new((byte)fontFamilies.Length, utf8FamilyNames, weight,
                isItalic ? (byte)1 : (byte)0);
        }
        finally
        {
            foreach (var utf8FamilyName in utf8FamilyNames)
            {
                if (utf8FamilyName != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(utf8FamilyName);
                }
            }
        }
    }
}