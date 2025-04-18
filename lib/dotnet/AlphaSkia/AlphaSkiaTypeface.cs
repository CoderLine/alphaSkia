using System.Text;

namespace AlphaSkia;

/// <summary>
/// Represents a typeface to draw text.
/// </summary>
public sealed class AlphaSkiaTypeface : AlphaSkiaNative
{
    private readonly AlphaSkiaData? _nativeData;
    private string? _fontFamily;

    /// <summary>
    /// Gets the name of the font family of this typeface.
    /// </summary>
    public string FamilyName
    {
        get
        {
            if (_fontFamily == null)
            {
                var skFamilyName = NativeMethods.alphaskia_typeface_get_family_name(Handle);
                var skFamilyChars = NativeMethods.alphaskia_string_get_utf8(skFamilyName);
                var skFamilyLength = NativeMethods.alphaskia_string_get_length(skFamilyName);
                unsafe
                {
                    _fontFamily = Encoding.UTF8.GetString((byte*)skFamilyChars.ToPointer(), (int)skFamilyLength);
                }
                NativeMethods.alphaskia_string_free(skFamilyName);
            }

            return _fontFamily;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the typeface is bold (weight >= 600, 600=>SemiBold)
    /// </summary>
    public bool IsBold => Weight >= 600 /* kSemiBold_Weight */;

    /// <summary>
    /// Gets the weight of the font.
    /// </summary>
    public ushort Weight =>  NativeMethods.alphaskia_typeface_get_weight(Handle);

    /// <summary>
    /// Gets a value indicating whether the typeface is italic.
    /// </summary>
    public bool IsItalic => NativeMethods.alphaskia_typeface_is_italic(Handle) != 0;

    private AlphaSkiaTypeface(IntPtr handle, AlphaSkiaData? nativeData = null)
        : base(handle, NativeMethods.alphaskia_typeface_free)
    {
        _nativeData = nativeData;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _nativeData?.Dispose();
        }
    }

    /// <summary>
    /// Register a new custom font from the given binary data containing the data of a font compatible with Skia (e.g. TTF).
    /// </summary>
    /// <param name="data">The raw binary data of the font.</param>
    /// <returns>The loaded typeface to use for text rendering or <code>null</code> if the loading failed.</returns>
    public static AlphaSkiaTypeface? Register(byte[] data)
    {
        var nativeData = new AlphaSkiaData(data);
        var typeface = NativeMethods.alphaskia_typeface_register(nativeData.Handle);
        if (typeface == IntPtr.Zero)
        {
            nativeData.Dispose();
            return null;
        }

        return new AlphaSkiaTypeface(typeface, nativeData);
    }

    /// <summary>
    /// Creates a typeface using the provided information.
    /// </summary>
    /// <param name="name">The name of the typeface.</param>
    /// <param name="weight">The weight of the typeface.</param>
    /// <param name="italic">Whether the italic version of the typeface should be loaded.</param>
    /// <returns>The typeface if it can be found in the already loaded fonts or the system fonts, otherwise <code>null</code>.</returns>
    public static AlphaSkiaTypeface? Create(string name, ushort weight, bool italic)
    {
        var typeface =
            NativeMethods.alphaskia_typeface_make_from_name(name, weight, italic ? (byte)1 : (byte)0);
        if (typeface == IntPtr.Zero)
        {
            return null;
        }

        return new AlphaSkiaTypeface(typeface);
    }

    /// <summary>
    /// Creates a typeface using the provided information.
    /// </summary>
    /// <param name="name">The name of the typeface.</param>
    /// <param name="bold">Whether the bold version of the typeface should be loaded.</param>
    /// <param name="italic">Whether the italic version of the typeface should be loaded.</param>
    /// <returns>The typeface if it can be found in the already loaded fonts or the system fonts, otherwise <code>null</code>.</returns>
    public static AlphaSkiaTypeface? Create(string name, bool bold, bool italic)
    {
        return Create(name, bold ? 700 : 400, italic);
    }
}