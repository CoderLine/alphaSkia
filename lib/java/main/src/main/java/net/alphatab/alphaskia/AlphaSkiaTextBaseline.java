package net.alphatab.alphaskia;

/**
 * Lists all vertical text baseline alignments which can be used to draw text.
 */
public enum AlphaSkiaTextBaseline {
    /**
     * The text is drawn using the alphabetic baseline.
     */
    ALPHABETIC(0),
    /**
     * The text is top-aligned to the provided position.
     */
    TOP(1),
    /**
     * The test is middle-aligned (vertically centered) to the provided position.
     */
    MIDDLE(2),

    /**
     * The text is bottom-aligned to the provided position.
     */
    BOTTOM(3);


    private final int value;

    int getValue() {
        return this.value;
    }

    AlphaSkiaTextBaseline(int value) {
        this.value = value;
    }
}
