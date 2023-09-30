package alphaskia;

/**
 * Lists all known color type with which AlphaSkia creates images.
 */
public enum AlphaSkiaColorType {
    // NOTE: we internally use kN32_SkColorType which is either  kBGRA_8888_SkColorType or kRGBA_8888_SkColorType
    // hence we don't need to expose the other SkColorType values

    /**
     * pixel with 8 bits for red, green, blue, alpha; in 32-bit word
     */
    RGBA_8888(4),

    /**
     * pixel with 8 bits for blue, green, red, alpha; in 32-bit word
     */
    BGRA_8888(6);

    private final int value;

    int getValue() {
        return value;
    }

    AlphaSkiaColorType(int value) {
        this.value = value;
    }

    public static AlphaSkiaColorType fromValue(int value) {
        if (value == RGBA_8888.getValue()) {
            return RGBA_8888;
        } else if (value == BGRA_8888.getValue()) {
            return BGRA_8888;
        }
        return null;
    }
}
