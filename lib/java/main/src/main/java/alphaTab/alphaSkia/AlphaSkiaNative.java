package alphaTab.alphaSkia;

/**
 * The base class for AlphaSkia objects wrapping native Skia objects.
 */
public abstract class AlphaSkiaNative implements AutoCloseable {
    static {
        if(!AlphaSkiaPlatform.isNativeLibLoaded()) {
            throw new IllegalStateException("Initialize the alphaSkia platform first");
        }
    }

    long handle;

    AlphaSkiaNative(long handle) {
        this.handle = handle;
    }


    @Override
    public abstract void close() throws Exception;
}
