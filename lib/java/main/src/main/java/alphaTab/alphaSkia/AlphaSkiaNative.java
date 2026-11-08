package alphaTab.alphaSkia;

/**
 * The base class for AlphaSkia objects wrapping native Skia objects.
 * Instances are not thread-safe: an object must not be used by one thread while another thread uses or closes it.
 * Closing an object more than once has no effect.
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

    /**
     * Detaches the native handle from this object, the caller becomes responsible for freeing it.
     * @return The native handle, or 0 if it was already detached.
     */
    synchronized long takeHandle() {
        long current = handle;
        handle = 0;
        return current;
    }


    @Override
    public abstract void close() throws Exception;
}
