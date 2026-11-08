package alphaTab.alphaSkia;

class AlphaSkiaData extends AlphaSkiaNative {
    AlphaSkiaData(long handle) {
        super(handle);
    }

    AlphaSkiaData(byte[] raw) {
        super(allocateCopy(raw));
    }

    private static native long allocateCopy(byte[] raw);

    @Override
    public void close() {
        release(takeHandle());
    }

    private static native void release(long handle);

    public native byte[] toArray();
}
