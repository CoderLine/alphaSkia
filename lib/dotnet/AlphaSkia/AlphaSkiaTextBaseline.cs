namespace AlphaSkia;

/// <summary>
/// Lists all vertical text baseline alignments which can be used to draw text.
/// </summary>
public enum AlphaSkiaTextBaseline
{
    /// <summary>
    /// The text is drawn using the alphabetic baseline.
    /// </summary>
    Alphabetic = 0,
    /// <summary>
    /// The text is top-aligned to the provided position.
    /// </summary>
    Top = 1,
    /// <summary>
    /// The test is middle-aligned (vertically centered) to the provided position.
    /// </summary>
    Middle = 2,
    /// <summary>
    /// The text is bottom-aligned to the provided position.
    /// </summary>
    Bottom = 3
}