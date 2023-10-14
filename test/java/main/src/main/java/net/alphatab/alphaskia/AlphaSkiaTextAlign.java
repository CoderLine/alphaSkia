package net.alphatab.alphaskia;

/**
 * Lists all text alignments which can be used to draw text.
 */
public enum AlphaSkiaTextAlign {
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

    private final int value;

    int getValue() {
        return this.value;
    }

    AlphaSkiaTextAlign(int value) {
        this.value = value;
    }
}
