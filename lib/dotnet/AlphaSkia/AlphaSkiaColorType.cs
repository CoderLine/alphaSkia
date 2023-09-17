namespace AlphaSkia;

/// <summary>
/// Lists all known color type with which AlphaSkia creates images.
/// </summary>
public enum AlphaSkiaColorType
{
    // NOTE: we internally use kN32_SkColorType which is either  kBGRA_8888_SkColorType or kRGBA_8888_SkColorType
    // hence we don't need to expose the other SkColorType values

    /// <summary>
    /// pixel with 8 bits for red, green, blue, alpha; in 32-bit word
    /// </summary>
    RGBA_8888 = 4,

    /// <summary>
    /// pixel with 8 bits for blue, green, red, alpha; in 32-bit word
    /// </summary>
    BGRA_8888 = 6,
}