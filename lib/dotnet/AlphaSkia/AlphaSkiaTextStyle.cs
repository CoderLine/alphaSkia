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
    public AlphaSkiaTextStyle(string[] fontFamilies, ushort weight, bool isItalic)
        : base(NativeMethods.alphaskia_textstyle_new((byte)fontFamilies.Length, fontFamilies, weight, isItalic ? (byte)1 : (byte)0), NativeMethods.alphaskia_textstyle_free)
    {
        FontFamilies = fontFamilies;
        Weight = weight;
        IsItalic = isItalic;
    }
}