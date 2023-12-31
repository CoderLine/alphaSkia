package alphaTab.alphaSkia

/**
 * Lists all text alignments which can be used to draw text.
 */
enum class AlphaSkiaTextAlign(val value:alphaskia_text_align_t) {
    /**
     * The text is drawn left aligned to the provided position.
     */
    LEFT(0),
    /**
     * The text is drawn centered to the provided position.
     */
    CENTER(1),
    /**
     * The text is drawn right aligned to the provided position.
     */
    RIGHT(2);
}
